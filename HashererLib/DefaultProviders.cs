using Hasherer;
using System.Security.Cryptography;
using System.Collections.Generic;
using SkeinFish;
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

    public class Skein256Instantiator : IProviderInstantiator
    {
        private static readonly ConfigurableStringValue NAME = new ConfigurableStringValue("HASH_NAME_SKEIN256", "Skein256");
        private static readonly ConfigurableValue<bool> IsEnabled = new ConfigurableValue<bool>("Skein256DefaultEnabled", new BooleanConverter(), false);
        public AsyncHashProvider Instantiate()
        {
            return new AsyncHashProvider(new Skein256(), NAME);
        }

        public bool DefaultEnabled
        {
            get { return IsEnabled; }
        }
    }

    public class Skein512Instantiator : IProviderInstantiator
    {
        private static readonly ConfigurableStringValue NAME = new ConfigurableStringValue("HASH_NAME_SKEIN512", "Skein512");
        private static readonly ConfigurableValue<bool> IsEnabled = new ConfigurableValue<bool>("Skein512DefaultEnabled", new BooleanConverter(), false);

        public AsyncHashProvider Instantiate()
        {
            return new AsyncHashProvider(new Skein512(), NAME);
        }

        public bool DefaultEnabled
        {
            get { return IsEnabled; }
        }
    }

    public class Skein1024Instantiator : IProviderInstantiator
    {
        private static readonly ConfigurableStringValue NAME = new ConfigurableStringValue("HASH_NAME_SKEIN1024", "Skein1024");
        private static readonly ConfigurableValue<bool> IsEnabled = new ConfigurableValue<bool>("Skein1024DefaultEnabled", new BooleanConverter(), false);
        public AsyncHashProvider Instantiate()
        {
            return new AsyncHashProvider(new Skein1024(), NAME);
        }

        public bool DefaultEnabled
        {
            get { return IsEnabled; }
        }
    }

}