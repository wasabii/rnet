using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Reflection;

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
        /// Invokes the specified function, ignoring exceptions of the specified type.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TException"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        public static TResult TryContinue<TResult, TException>(Func<TResult> func)
            where TException : Exception
        {
            try
            {
                return func();
            }
            catch (TException)
            {
                return default(TResult);
            }
        }

        /// <summary>
        /// Invokes the specified function, ignoring exceptions of the specified type.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TException"></typeparam>
        /// <param name="func"></param>
        /// <param name="onException"></param>
        /// <returns></returns>
        public static TResult TryCatch<TResult, TException>(Func<TResult> func, Action<TException> onException)
            where TException : Exception
        {
            try
            {
                return func();
            }
            catch (TException e)
            {
                onException(e);
                return default(TResult);
            }
        }

    }

}
