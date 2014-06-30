﻿using System;
using System.Diagnostics.Contracts;

namespace Rnet.Drivers.Default
{

    /// <summary>
    /// Serves as a simple profile implementation base for the local device.
    /// </summary>
    public abstract class LogicalObjectBase : ProfileBase
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="target"></param>
        protected LogicalObjectBase(LogicalBusObject target)
            : base(target)
        {
            Contract.Requires<ArgumentNullException>(target != null); 
        }

        /// <summary>
        /// The object implementing this profile.
        /// </summary>
        protected LogicalBusObject Object
        {
            get { return (LogicalBusObject)Target; }
        }

    }

}
