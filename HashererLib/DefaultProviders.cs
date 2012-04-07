using Hasherer;
using System.Security.Cryptography;
using System.Collections.Generic;
using T0yK4T.Configuration;
using System;

namespace HashererLib
{
    public abstract class InstantiatorBase : IProviderInstantiator
    {
        private ConfigurableValue<string> name;
        private ConfigurableValue<bool> isEnabled;

        protected InstantiatorBase(string name, bool defEnabled)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");
            this.name = Config.GetValue<string>(string.Format("{0}Name", name), new StringConverter(), name);
            this.isEnabled = Config.GetValue<bool>(string.Format("{0}DefaultEnabled", name), new BooleanConverter(), defEnabled);
        }

        public AsyncHashProvider Instantiate()
        {
            return new AsyncHashProvider(this.GetAlgorithm(), this.DisplayName);
        }

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
    /// <summary>
    /// </summary>
    public class Crc32Instantiator : InstantiatorBase
    {
        public Crc32Instantiator()
            : base("CRC32", true) { }

        protected override HashAlgorithm GetAlgorithm()
        {
            return new Crc32();
        }
    }

    public class Md5Instantiator : InstantiatorBase
    {
        public Md5Instantiator()
            : base("MD5", true) { }
    
        protected override HashAlgorithm GetAlgorithm()
        {
            return MD5.Create();
        }
    }

    public class Sha1Instantiator : InstantiatorBase
    {
        public Sha1Instantiator()
            : base("SHA1", true) { }
        protected override HashAlgorithm GetAlgorithm()
        {
            return SHA1.Create();
        }
    }

    public class Sha256Instantiator : InstantiatorBase
    {
        public Sha256Instantiator()
            : base("SHA256", false) { }
        protected override HashAlgorithm GetAlgorithm()
        {
            return SHA256.Create();
        }
    }

    public struct TestStruct : IProviderInstantiator
    {
        public AsyncHashProvider Instantiate()
        {
            throw new NotImplementedException();
        }

        public bool DefaultEnabled
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string DisplayName
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }

}