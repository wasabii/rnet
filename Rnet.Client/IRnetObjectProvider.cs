using System;

namespace Rnet.Client
{

    /// <summary>
    /// Provides the ability to request data from an RNET Uri.
    /// </summary>
    public interface IRnetObjectProvider
    {

        /// <summary>
        /// Gets the specified object.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        RnetRef GetAsync(Uri uri);

    }

}
