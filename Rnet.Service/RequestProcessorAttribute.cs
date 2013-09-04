using System;
using System.ComponentModel.Composition;

namespace Rnet.Service.Processors
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

    }

    [AttributeUsage(AttributeTargets.Class)]
    [MetadataAttribute]
    public class RequestProcessorAttribute : ExportAttribute, IRequestProcessorMetadata
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public RequestProcessorAttribute(Type type)
            : base(typeof(IRequestProcessor))
        {
            Type = type;
        }

        /// <summary>
        /// Type of objects to handle.
        /// </summary>
        public Type Type { get; private set; }

    }

}
