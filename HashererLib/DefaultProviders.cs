using Hasherer;
using System.Security.Cryptography;
using System.Collections.Generic;
using T0yK4T.Configuration;

namespace HasherLib
{
    /// <summary>
    /// </summary>
    public class Crc32Instantiator : IProviderInstantiator
    {
        private static readonly ConfigurableStringValue NAME = new ConfigurableStringValue("HASH_NAME_CRC32", "CRC32");
        private static readonly ConfigurableValue<bool> IsEnabled = new ConfigurableValue<bool>("Crc32DefaultEnabled", new BooleanConverter(), true);
        public AsyncHashProvider Instantiate()
        {
            return new AsyncHashProvider(new Crc32(), NAME);
        }

        public bool DefaultEnabled
        {
            get { return IsEnabled; }
        }
    }

    public class Md5Instantiator : IProviderInstantiator
    {
        private static readonly ConfigurableStringValue NAME = new ConfigurableStringValue("HASH_NAME_MD5", "MD5");
        private static readonly ConfigurableValue<bool> IsEnabled = new ConfigurableValue<bool>("Md5DefaultEnabled", new BooleanConverter(), true);
        public AsyncHashProvider Instantiate()
        {
            return new AsyncHashProvider(MD5.Create(), NAME);
        }

        public bool DefaultEnabled
        {
            get { return IsEnabled; }
        }
    }

    public class Sha1Instantiator : IProviderInstantiator
    {
        private static readonly ConfigurableValue<bool> IsEnabled = new ConfigurableValue<bool>("Sha1DefaultEnabled", new BooleanConverter(), true);
        private static readonly ConfigurableStringValue NAME = new ConfigurableStringValue("HASH_NAME_SHA1", "SHA1");
        public AsyncHashProvider Instantiate()
        {
            return new AsyncHashProvider(SHA1.Create(), NAME);
        }

        public bool DefaultEnabled
        {
            get { return IsEnabled; }
        }
    }

    public class Sha256Instantiator : IProviderInstantiator
    {
        private static readonly ConfigurableValue<bool> IsEnabled = new ConfigurableValue<bool>("Sha256DefaultEnabled", new BooleanConverter(), true);
        private static readonly ConfigurableStringValue NAME = new ConfigurableStringValue("HASH_NAME_SHA256", "SHA256");
        public AsyncHashProvider Instantiate()
        {
            return new AsyncHashProvider(SHA256.Create(), NAME);
        }

        public bool DefaultEnabled
        {
            get { return IsEnabled; }
        }
    }
}