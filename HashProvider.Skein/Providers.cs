using Hasherer;
using T0yK4T.Configuration;
using SkeinFish;
using HashererLib;
using System.Security.Cryptography;

namespace HashProvider.Skein
{
#pragma warning disable 1591
    /// <summary>
    /// Used to intialize Skein256 provider
    /// </summary>
    public class Skein256Instantiator : InstantiatorBase
    {
        public Skein256Instantiator()
            : base("Skein256", false) { }
        protected override HashAlgorithm GetAlgorithm()
        {
            return new Skein256();
        }
    }

    /// <summary>
    /// Used to intialize Skein512 provider
    /// </summary>
    public class Skein512Instantiator : InstantiatorBase
    {
        public Skein512Instantiator()
            : base("Skein512", false) { }
        protected override HashAlgorithm GetAlgorithm()
        {
            return new Skein512();
        }
    }

    /// <summary>
    /// Used to intialize Skein1024 provider
    /// </summary>
    public class Skein1024Instantiator : InstantiatorBase
    {
        public Skein1024Instantiator()
            : base("Skein1024", false) { }
        protected override HashAlgorithm GetAlgorithm()
        {
            return new Skein1024();
        }
    }
#pragma warning restore 1591
}