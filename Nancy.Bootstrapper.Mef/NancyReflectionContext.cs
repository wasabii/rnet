using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Context;

namespace Nancy.Bootstrapper.Mef
{

    /// <summary>
    /// Provides a custom 
    /// </summary>
    public class NancyReflectionContext : CustomReflectionContext
    {

        protected override IEnumerable<object> GetCustomAttributes(MemberInfo member, IEnumerable<object> declaredAttributes)
        {
            return base.GetCustomAttributes(member, declaredAttributes);
        }

    }

}
