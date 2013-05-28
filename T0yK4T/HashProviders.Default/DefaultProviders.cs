using Hasherer;
using System.Security.Cryptography;
using System.Collections.Generic;
using System;
using Com.Damieng;

namespace Hasherer.DefaultProviders
{
#pragma warning disable 1591
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
#pragma warning restore 1591
}