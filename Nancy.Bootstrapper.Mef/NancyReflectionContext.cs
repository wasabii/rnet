using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Reflection.Context;

namespace Nancy.Bootstrapper.Mef
{

    /// <summary>
    /// Provides custom attributes for Nancy types that are not decorated with MEF attributes.
    /// </summary>
    public class NancyReflectionContext : CustomReflectionContext
    {

        static readonly Assembly nancyAssembly = typeof(NancyEngine).Assembly;

        /// <summary>
        /// Caches all the attributes for an object.
        /// </summary>
        ConcurrentDictionary<object, IEnumerable<object>> attributes =
            new ConcurrentDictionary<object, IEnumerable<object>>();

        /// <summary>
        /// Returns <c>true</c> if the specified member is already an export.
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        static bool IsMefExport(MemberInfo member)
        {
            Contract.Requires<ArgumentNullException>(member != null);

            var type = member as Type ?? member.DeclaringType;
            if (type == null)
                return false;

            // is the type decorated with any MEF attributes; if so, it's already an export
            return Enumerable.Empty<Attribute>()
                .Concat(type.GetCustomAttributes<ExportAttribute>(true))
                .Concat(type.GetCustomAttributes<InheritedExportAttribute>(true))
                .Concat(type.GetMembers(BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                    .SelectMany(i => Enumerable.Empty<Attribute>()
                        .Concat(i.GetCustomAttributes<ExportAttribute>(true))
                        .Concat(i.GetCustomAttributes<InheritedExportAttribute>(true))))
                .Any();
        }

        /// <summary>
        /// Returns whether or not the specified type is an ImportMany.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        static Attribute GetImportAttribute(Type type)
        {
            Contract.Requires<ArgumentNullException>(type != null);

            // collection types only allow a single generic
            if (type.GetGenericArguments().Length > 1)
                return new ImportAttribute();

            // check for supported collection types
            var openType = type.IsGenericType() ? type.GetGenericTypeDefinition() : type;
            if (openType == typeof(IEnumerable) ||
                openType == typeof(IEnumerable<>) ||
                openType == typeof(ICollection) ||
                openType == typeof(ICollection<>) ||
                openType.IsArray)
                return new ImportManyAttribute();

            return new ImportAttribute();
        }

        /// <summary>
        /// Returns the Nancy contracts implemented by the specified type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        static IEnumerable<Type> GetExportContracts(Type type)
        {
            Contract.Requires<ArgumentNullException>(type != null);

            if (type.IsClass &&
                type.IsPublic &&
                GetImportConstructor(type) != null) /* required to export */
            {
                // export as self
                //yield return type;

                // export each Nancy interface
                foreach (var contractType in type.GetInterfaces())
                    if (contractType.Assembly == nancyAssembly &&
                        contractType.IsInterface &&
                        contractType.IsPublic)
                        yield return contractType;
            }
        }

        /// <summary>
        /// Returns the <see cref="ConstructorInfo"/> for the given type that should be used to create the instance.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        static ConstructorInfo GetImportConstructor(Type type)
        {
            Contract.Requires<ArgumentNullException>(type != null);

            return type.GetConstructors(BindingFlags.Instance | BindingFlags.Public)
                .OrderByDescending(i => i.GetParameters().Length)
                .FirstOrDefault();
        }

        bool Exclude(Type type)
        {
            // ignore non-nancy assemblies
            var assemblyName = type.Assembly.GetName().Name;
            if (assemblyName != "Nancy" && !assemblyName.StartsWith("Nancy."))
                return true;

            // we implement these, but cannot export them
            if (type.IsInterface)
                return true;

            // skip
            if (type.IsSubclassOf(typeof(Exception)))
                return true;

            // type already has exports, must not be Nancy related
            if (IsMefExport((MemberInfo)type))
                return true;

            return false;
        }

        protected override IEnumerable<object> GetCustomAttributes(MemberInfo member, IEnumerable<object> declaredAttributes)
        {
            return attributes.GetOrAdd(member, _ =>
            {
                // resolve type; on error simply let it continue
                var type = member as Type ?? member.DeclaringType;
                if (type == null)
                    return Enumerable.Empty<object>();

                // excludes the type for varous reasons
                if (Exclude(type))
                    return Enumerable.Empty<object>();

                // return base attributes and our own
                return base.GetCustomAttributes(member, declaredAttributes)
                    .Concat(GetCustomAttributes(type, member))
                    .ToList();
            });
        }

        IEnumerable<object> GetCustomAttributes(Type type, MemberInfo member)
        {
            if (member is Type)
                return GetCustomAttributes((Type)member);
            if (member is ConstructorInfo)
                return GetCustomAttributes(type, (ConstructorInfo)member);

            return Enumerable.Empty<object>();
        }

        protected override IEnumerable<object> GetCustomAttributes(ParameterInfo parameter, IEnumerable<object> declaredAttributes)
        {
            return attributes.GetOrAdd(parameter, _ =>
            {
                var type = parameter.Member.ReflectedType;
                if (type == null)
                    return Enumerable.Empty<object>();

                // excludes the type for varous reasons
                if (Exclude(type))
                    return Enumerable.Empty<object>();

                // return base attributes and our own
                return base.GetCustomAttributes(parameter, declaredAttributes)
                    .Concat(GetCustomAttributes(type, parameter))
                    .ToList();
            });
        }

        /// <summary>
        /// Provides additional attributes for a Nancy export.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        IEnumerable<object> GetCustomAttributes(Type type)
        {
            var name = type.Name;

            var contractTypes = GetExportContracts(type);
            if (contractTypes.Any())
                yield return new NancyGeneratedPartAttribute();

            // export as each available contract type
            foreach (var contractType in contractTypes)
                yield return new NancyExportAttribute(contractType);
        }

        /// <summary>
        /// Provides additional attributes for a Nancy export constructor.
        /// </summary>
        /// <param name="constructor"></param>
        /// <returns></returns>
        IEnumerable<object> GetCustomAttributes(Type type, ConstructorInfo constructor)
        {
            var name = type.FullName;

            if (!GetExportContracts(type).Any())
                yield break;

            // we are the matching constructor
            if (GetImportConstructor(type) == constructor)
                yield return new ImportingConstructorAttribute();
        }

        /// <summary>
        /// Provides additional attributes for a Nancy constructor parameter.
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        IEnumerable<object> GetCustomAttributes(Type type, ParameterInfo parameter)
        {
            // we are the matching constructor
            if (GetImportConstructor(type) != (ConstructorInfo)parameter.Member)
                yield break;

            yield return GetImportAttribute(parameter.ParameterType);
        }

    }

}
