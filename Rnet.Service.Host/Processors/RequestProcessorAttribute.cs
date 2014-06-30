using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Rnet.Service.Host.Processors
{

    /// <summary>
    /// Metadata for <see cref="IRequestProcessor"/>s.
    /// </summary>
    public interface IRequestProcessorMetadata
    {

        /// <summary>
        /// Type of objects to handle.
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Higher priority processors are given a chance first.
        /// </summary>
        int Priority { get; }

    }

    [AttributeUsage(AttributeTargets.Class)]
    [MetadataAttribute]
    public class RequestProcessorAttribute :
        ExportAttribute,
        IRequestProcessorMetadata
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public RequestProcessorAttribute(Type type)
            : this(type, 0)
        {

        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public RequestProcessorAttribute(Type type, int priority)
            : base(typeof(IRequestProcessor))
        {
            Type = type;
            Priority = priority;
        }

        /// <summary>
        /// Type of objects to handle.
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// Higher priority processors are given a chance first.
        /// </summary>
        public int Priority { get; private set; }

    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    [MetadataAttribute]
    public class RequestProcessorMultipleAttribute :
        Attribute,
        IRequestProcessorMetadata
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public RequestProcessorMultipleAttribute(Type type)
            : this(type, 0)
        {
            Contract.Requires<ArgumentNullException>(type != null);
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public RequestProcessorMultipleAttribute(Type type, int priority)
        {
            Contract.Requires<ArgumentNullException>(type != null);

            Type = type;
            Priority = priority;
        }

        /// <summary>
        /// Type of objects to handle.
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// Higher priority processors are given a chance first.
        /// </summary>
        public int Priority { get; private set; }

    }

    /// <summary>
    /// Serves as a view for <see cref="IRequestProcessorMetadata"/>.
    /// </summary>
    public sealed class RequestProcessorMetadata
    {

        /// <summary>
        /// Extracts an array from the given object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="o"></param>
        /// <returns></returns>
        static T[] AsArray<T>(object o)
        {
            if (o is T[])
                return (T[])o;
            else if (o is IEnumerable<T>)
                return ((IEnumerable<T>)o).ToArray();
            else if (o is T)
                return new T[] { (T)o };
            else
                return new T[] { };
        }

        readonly IRequestProcessorMetadata[] infos;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="metadata"></param>
        public RequestProcessorMetadata(IDictionary<string, object> metadata)
        {
            Contract.Requires<ArgumentNullException>(metadata != null);

            // get metadata arrays
            var p1 = AsArray<Type>(metadata["Type"]);
            var p2 = AsArray<int>(metadata["Priority"]);

            // generate metadata pairs
            this.infos = p1.Zip(p2, (i, j) => new RequestProcessorMultipleAttribute(i, j)).ToArray();
        }

        public IEnumerable<IRequestProcessorMetadata> Infos
        {
            get { return infos; }
        }

    }

}
