using T0yK4T.Tools;
using System.Security.Cryptography;
using T0yK4T.Configuration;
using System;

namespace Hasherer
{
    /// <summary>
    /// Base class from which Provider Instantiators can derrive
    /// </summary>
    public abstract class InstantiatorBase : IProviderInstantiator
    {
        private ConfigurableValue<string> name;
        private ConfigurableValue<bool> isEnabled;

        /// <summary>
        /// Initializes a new <see cref="InstantiatorBase"/> and sets the DisplayName and DefaultEnabled properties to the specified values
        /// <para/>
        /// Note that the DisplayName and DefaultEnabled properties are infact <see cref="ConfigurableValue{T}"/>s,
        /// <para/>
        /// only if either of the values is not stored in the configuration will it's value be set to the default value specified in this constructor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defEnabled"></param>
        protected InstantiatorBase(string name, bool defEnabled)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");
            this.name = Config.GetValue<string>(string.Format("{0}Name", name), new StringConverter(), name);
            this.isEnabled = Config.GetValue<bool>(string.Format("{0}DefaultEnabled", name), new BooleanConverter(), defEnabled);
        }

        /// <summary>
        /// Gets a new <see cref="AsyncHashProvider"/>
        /// </summary>
        /// <returns></returns>
        public AsyncHashProvider Instantiate()
        {
            return new AsyncHashProvider(this.GetAlgorithm(), this.DisplayName);
        }

        /// <summary>
        /// When overriden, should return the <see cref="HashAlgorithm"/> to use for the <see cref="AsyncHashProvider"/>
        /// </summary>
        /// <returns></returns>
        protected abstract HashAlgorithm GetAlgorithm();

        /// <summary>
        /// Gets or Sets a value indicating wether or not the provider created with this instantiator is enabled by default
        /// </summary>
        public bool DefaultEnabled
        {
            get
            {
                return isEnabled;
            }
            set
            {
                this.isEnabled.Value = value;
                this.isEnabled.Store();
            }
        }


        /// <summary>
        /// Gets or Sets the display name of the hash produced using this instantiator
        /// </summary>
        public string DisplayName
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name.Value = value;
                this.name.Store();
            }
        }
    }
}