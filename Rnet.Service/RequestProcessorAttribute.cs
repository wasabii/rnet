﻿using System;
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

}