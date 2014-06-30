using System;
using System.CodeDom.Compiler;
using System.Diagnostics.Contracts;
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
            Contract.Requires<ArgumentNullException>(writer != null);

            var wrt = new IndentedTextWriter(writer, "    ");
            wrt.Indent = 1;

            // .NET hack
            var field = typeof(IndentedTextWriter).GetField("tabsPending", BindingFlags.Instance | BindingFlags.NonPublic);
            if (field != null)
                field.SetValue(wrt, true);

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
            Contract.Requires<ArgumentNullException>(func != null);
            Contract.Requires<ArgumentNullException>(cancellationTokens != null);

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
