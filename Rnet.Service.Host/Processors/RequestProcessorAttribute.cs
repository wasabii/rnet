using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

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
    public class RequestProcessorAttribute : ExportAttribute, IRequestProcessorMetadata
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
    public class RequestProcessorMultipleAttribute : Attribute, IRequestProcessorMetadata
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public RequestProcessorMultipleAttribute(Type type)
            : this(type, 0)
        {

        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public RequestProcessorMultipleAttribute(Type type, int priority)
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

    /// <summary>
    /// Serves as a view for <see cref="IRequestProcessorMetadata"/>.
    /// </summary>
    public sealed class RequestProcessorMetadata
    {

        public IRequestProcessorMetadata[] Infos { get; set; }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="d"></param>
        public RequestProcessorMetadata(IDictionary<string, object> d)
        {
            var p1 = d["Type"] as Type[] ?? new Type[] { (Type)d["Type"] };
            var p2 = d["Priority"] as int[] ?? new int[] { (int)d["Priority"] };

            Infos = new RequestProcessorMultipleAttribute[p1.Length];
            for (int i = 0; i < Infos.Length; i++)
                Infos[i] = new RequestProcessorMultipleAttribute(p1[i], p2[i]);
        }
    }

}
