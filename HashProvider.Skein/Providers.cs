using Hasherer;
using T0yK4T.Configuration;
using SkeinFish;

namespace HashProvider.Skein
{
    /// <summary>
    /// Used to intialize Skein256 provider
    /// </summary>
    public class Skein256Instantiator : IProviderInstantiator
    {
        private static readonly ConfigurableStringValue NAME = new ConfigurableStringValue("HASH_NAME_SKEIN256", "Skein256");
        private static readonly ConfigurableValue<bool> IsEnabled = new ConfigurableValue<bool>("Skein256DefaultEnabled", new BooleanConverter(), false);

        /// <summary>
        /// Instantiates a new AsyncHashProvider with a Skein256 algorithm
        /// </summary>
        /// <returns></returns>
        public AsyncHashProvider Instantiate()
        {
            return new AsyncHashProvider(new Skein256(), NAME);
        }

        /// <summary>
        /// Gets a value indicating whether or not the provider should be enabled by default
        /// </summary>
        public bool DefaultEnabled
        {
            get { return IsEnabled; }
        }
    }

    /// <summary>
    /// Used to intialize Skein512 provider
    /// </summary>
    public class Skein512Instantiator : IProviderInstantiator
    {
        private static readonly ConfigurableStringValue NAME = new ConfigurableStringValue("HASH_NAME_SKEIN512", "Skein512");
        private static readonly ConfigurableValue<bool> IsEnabled = new ConfigurableValue<bool>("Skein512DefaultEnabled", new BooleanConverter(), false);

        /// <summary>
        /// Instantiates a new AsyncHashProvider with a Skein512 algorithm
        /// </summary>
        /// <returns></returns>
        public AsyncHashProvider Instantiate()
        {
            return new AsyncHashProvider(new Skein512(), NAME);
        }

        /// <summary>
        /// Gets a value indicating whether or not the provider should be enabled by default
        /// </summary>
        public bool DefaultEnabled
        {
            get { return IsEnabled; }
        }
    }

    /// <summary>
    /// Used to intialize Skein1024 provider
    /// </summary>
    public class Skein1024Instantiator : IProviderInstantiator
    {
        private static readonly ConfigurableStringValue NAME = new ConfigurableStringValue("HASH_NAME_SKEIN1024", "Skein1024");
        private static readonly ConfigurableValue<bool> IsEnabled = new ConfigurableValue<bool>("Skein1024DefaultEnabled", new BooleanConverter(), false);

        /// <summary>
        /// Instantiates a new AsyncHashProvider with a Skein1024 algorithm
        /// </summary>
        /// <returns></returns>
        public AsyncHashProvider Instantiate()
        {
            return new AsyncHashProvider(new Skein1024(), NAME);
        }

        /// <summary>
        /// Gets a value indicating whether or not the provider should be enabled by default
        /// </summary>
        public bool DefaultEnabled
        {
            get { return IsEnabled; }
        }
    }
}