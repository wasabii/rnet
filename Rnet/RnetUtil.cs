using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Rnet
{

    /// <summary>
    /// Various utilities.
    /// </summary>
    static class RnetUtil
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

        /// <summary>
        /// Executes the function returning the default value if cancelled.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <param name="cancellationTokens"></param>
        /// <returns></returns>
        public static async Task<T> DefaultIfCancelled<T>(Func<CancellationToken, Task<T>> func, params CancellationToken[] cancellationTokens)
        {
            try
            {
                return await func(CancellationTokenSource.CreateLinkedTokenSource(cancellationTokens).Token);
            }
            catch (OperationCanceledException)
            {
                // ignore
            }

            return default(T);
        }

    }

}
