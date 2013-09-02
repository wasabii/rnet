using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;

namespace Rnet.Service
{
    public class TsetModule : NancyModule
    {

        public TsetModule()
            : base("/foo")
        {
            Before.AddItemToStartOfPipeline(x => Response.AsText("hi"));
            Get["/"] = x => "Hello!";
        }
    }
}
