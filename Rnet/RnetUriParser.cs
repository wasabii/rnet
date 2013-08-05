using System;

namespace Rnet
{

    /// <summary>
    /// Base RNET UriParser class.
    /// </summary>
    public abstract class RnetUriParser : GenericUriParser
    {

        /// <summary>
        /// Initializes the static instance.
        /// </summary>
        static RnetUriParser()
        {
            RegisterParsers();
        }

        /// <summary>
        /// Registers the URI parsers.
        /// </summary>
        public static void RegisterParsers()
        {
            RnetTcpUriParser.RegisterParser();
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        protected RnetUriParser()
            : base(GenericUriParserOptions.Default | GenericUriParserOptions.NoFragment)
        {

        }

    }

}
