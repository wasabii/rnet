using System.CodeDom.Compiler;
using System.IO;
using System.Reflection;

namespace Rnet.Protocol
{

    public static class RnetUtils
    {   

        /// <summary>
        /// Creates a new indented text writer.
        /// </summary>
        /// <param name="writer"></param>
        /// <returns></returns>
        public static IndentedTextWriter CreateIndentedTextWriter(TextWriter writer)
        {
            var wrt = new IndentedTextWriter(writer, "    ");
            wrt.Indent = 1;
            typeof(IndentedTextWriter).GetField("tabsPending", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(wrt, true);
            return wrt;
        }

    }

}
