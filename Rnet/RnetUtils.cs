using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Rnet
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

        /// <summary>
        /// Attempts to execute the given function. Throws exceptions only if the first <see cref="CancellationToken"/>
        /// is cancelled, or unknown exceptions occur.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="func"></param>
        /// <param name="fatal"></param>
        /// <param name="exit"></param>
        /// <returns></returns>
        public static async Task<TResult> TryTask<TResult>(Func<Task<TResult>> func, CancellationToken fatal, CancellationToken exit)
        {
            try
            {
                return await func();
            }
            catch (OperationCanceledException e)
            {
                if (exit.IsCancellationRequested)
                    return default(TResult);
                else if (fatal.IsCancellationRequested)
                    fatal.ThrowIfCancellationRequested();
                else
                    throw e;
            }

            return default(TResult);
        }

    }

}
