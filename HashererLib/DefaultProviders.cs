using Hasherer;
using System.Security.Cryptography;
using System.Collections.Generic;

namespace HasherLib
{
    /// <summary>
    /// Default implementation of <see cref="IProviderInstantiator"/>
    /// </summary>
    public class DefaultProviders : IProviderInstantiator
    {
        /// <summary>
        /// <see cref="IProviderInstantiator.Instantiate()"/>
        /// </summary>
        /// <returns></returns>
        IEnumerable<AsyncHashProvider> IProviderInstantiator.Instantiate()
        {
            return new AsyncHashProvider[]
            {
                new AsyncHashProvider(new Crc32(), "CRC32"),
                new AsyncHashProvider(MD5.Create(), "MD5"),
                new AsyncHashProvider(SHA1.Create(), "SHA1"),
                new AsyncHashProvider(SHA256.Create(), "SHA256"),
            };
        }
    }
}