using Hasherer;
using System.Security.Cryptography;
using System.Collections.Generic;
using SkeinFish;
using T0yK4T.Configuration;

namespace HasherLib
{
    /// <summary>
    /// Default implementation of <see cref="IProviderInstantiator"/>
    /// </summary>
    public class Crc32Instantiator : IProviderInstantiator
    {
        //private static readonly ConfigurableStringValue CRC32_NAME = new ConfigurableStringValue("HASH_NAME_CRC32", "CRC32");
        //private static readonly ConfigurableStringValue MD5_NAME = new ConfigurableStringValue("HASH_NAME_MD5", "MD5");
        //private static readonly ConfigurableStringValue SHA1_NAME = new ConfigurableStringValue("HASH_NAME_SHA1", "SHA1");
        //private static readonly ConfigurableStringValue SHA256_NAME = new ConfigurableStringValue("HASH_NAME_SHA256", "SHA256");
        //private static readonly ConfigurableStringValue SKEIN256_NAME = new ConfigurableStringValue("HASH_NAME_SKEIN256", "Skein256");
        //private static readonly ConfigurableStringValue SKEIN512_NAME = new ConfigurableStringValue("HASH_NAME_SKEIN512", "Skein512");
        //private static readonly ConfigurableStringValue SKEIN1024_NAME = new ConfigurableStringValue("HASH_NAME_SKEIN1024", "Skein1024");

        ///// <summary>
        ///// <see cref="IProviderInstantiator.Instantiate()"/>
        ///// </summary>
        ///// <returns></returns>
        //public IEnumerable<AsyncHashProvider> Instantiate()
        //{
        //    return new AsyncHashProvider[]
        //    {
        //        new AsyncHashProvider(MD5.Create(), "MD5"),
        //        new AsyncHashProvider(SHA1.Create(), "SHA1"),
        //        new AsyncHashProvider(SHA256.Create(), "SHA256"),
        //        new AsyncHashProvider(new Skein256(), "Skein256"),
        //        new AsyncHashProvider(new Skein512(), "Skein512"),
        //        new AsyncHashProvider(new Skein1024(), "Skein1024"),
        //    };
        //}

        public AsyncHashProvider Instantiate()
        {
            return new AsyncHashProvider(new Crc32(), "CRC32");
        }

        public bool DefaultEnabled
        {
            get { return true; }
        }
    }
}