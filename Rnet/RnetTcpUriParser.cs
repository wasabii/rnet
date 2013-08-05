using System;

namespace Rnet
{

    /// <summary>
    /// Parses URIs of the form rnet.tcp://hostname:port.
    /// </summary>
    public class RnetTcpUriParser : RnetUriParser
    {

        /// <summary>
        /// Registers the URI parser.
        /// </summary>
        public static void RegisterParser()
        {
            // do nothing, static initializer will take care of it
        }

        /// <summary>
        /// Registers the URI parser.
        /// </summary>
        static void RegisterUriParser()
        {
            UriParser.Register(new RnetTcpUriParser(), "rnet.tcp", 9999);
        }

    }

}
