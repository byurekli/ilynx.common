using System;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Cryptography;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using T0yK4T.Tools;
using System.Diagnostics;

namespace T0yK4T.Cryptography
{
	/// <summary>
	/// Contains helper methods for the RSACryptoServiceProvider
	/// </summary>
    public class RSAHelper : ComponentBase, IDisposable
    {
#if DEBUG
        private static int runningInstances = 0;
#endif
        private const int KEY_SIZE = 4096; //3072; // This "should" be enough until 2030 according to RSA...
        private RSAParameters rsaParams;
        private IFormatter bFormatter;
        private MemoryStream mStream;
        private ILogger logger;
        private int MAX_INPUT_BYTES = 0;
        private RSACryptoServiceProvider provider;
        static Timer t;

        /// <summary>
        /// This method will take care of generating a public/private key set,
        /// <para/>
        /// since this is an expensive, and usually time-consuming task,
        /// <para/>
        /// it is suggested to run this method before any actual use of this class
        /// </summary>
        /// <returns></returns>
        public static CspParameters GenKeys()
        {
            Console.WriteLine("RSAHelper: generating keys and other goodies...");
            CspParameters parameters = new CspParameters();
            RSACryptoServiceProvider provider = new RSACryptoServiceProvider();
            parameters.Flags = CspProviderFlags.NoPrompt | CspProviderFlags.UseMachineKeyStore |CspProviderFlags.UseExistingKey;
            parameters.KeyNumber = (int)KeyNumber.Exchange;
            
#if DEBUG
            if (runningInstances > 0)
                parameters.KeyContainerName = Guid.NewGuid().ToString();
            else
#endif
                parameters.KeyContainerName = System.Reflection.Assembly.GetCallingAssembly().FullName; // typeof(RSAHelper).FullName;

            try
            {
                Console.WriteLine("RSAHelper: Attempting to open existing key container");
                provider = new RSACryptoServiceProvider(parameters);
                RSAParameters pa = provider.ExportParameters(false);
                if (pa.Modulus.Length * 8 != KEY_SIZE)
                {
                    Console.WriteLine("Found existing key ({0}), but not of the correct size ({1})", pa.Modulus.Length * 8, KEY_SIZE);
                    provider.PersistKeyInCsp = false;
                    provider.Clear();
                    GenRSA(parameters, ref provider);
                }
            }
            catch
            {
                Console.WriteLine("No existing Key Container was found in the machine keystore");
                GenRSA(parameters, ref provider);
            }
            provider = null;
            GC.Collect();
            return parameters;
        }

        /// <summary>
        /// Default destructor
        /// <para/>
        /// "cleaning up" sensitive data
        /// <para/>
        /// Please note that this may not be called until the garbage collector decides to call it.
        /// <para/>
        /// Thus, calling GC.Collect() after disposal may be a good idea
        /// <para/>
        /// Note: This executes the "Dispose" method implemented from <see cref="IDisposable"/>
        /// </summary>
        ~RSAHelper()
        {
#if DEBUG
            runningInstances--;
#endif
            try
            {
                this.Dispose();
            }
            catch { }
        }

        private class Ticker
        {
            int count = 0;
            public void Tick(object state)
            {
                Console.Write(".");
                count++;
                if (count >= 50)
                {
                    Console.WriteLine();
                    count = 0;
                    
                }
            }
        }

        private static void GenRSA(CspParameters parameters, ref RSACryptoServiceProvider provider)
        {
            parameters.Flags = CspProviderFlags.NoPrompt | CspProviderFlags.UseMachineKeyStore;
            Ticker r = new Ticker();
            t = new Timer(r.Tick, null, 1000, 1000);
            int size = KEY_SIZE;
#if DEBUG
            if (runningInstances > 1)
                size = 1024;
#endif
            Console.WriteLine("RSAHelper: Generating {0} bit RSA Keyset", size);
            Console.WriteLine("RSAHelper: This will probably take a while");
            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                provider = new RSACryptoServiceProvider(size, parameters);
                sw.Stop();
                t.Change(Timeout.Infinite, Timeout.Infinite);
                Console.WriteLine();
                Console.WriteLine("RSAHelper: Took approximately {1} seconds to generate our {0} bit long key", size, sw.Elapsed.TotalSeconds.ToString("f2"));
                t = null;
            }
            catch (Exception e)
            {
                t.Change(Timeout.Infinite, Timeout.Infinite);
                t = null;
                Console.WriteLine();
                Console.WriteLine(e.ToString());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="pubKey"></param>
        public RSAHelper(ILogger logger, string pubKey)
        {
            this.logger = logger;
            this.InitPubOnly(pubKey);
        }

        /// <summary>
        /// Initializes a new instance of RSAHelper and sets the Public Key of the internal RSACryptoServiceProvider to the specified value
        /// </summary>
        /// <param name="pubKey"></param>
        public RSAHelper(string pubKey)
        {
            this.InitPubOnly(pubKey);
        }

        private void InitPubOnly(string pubKey)
        {
            this.provider = new RSACryptoServiceProvider();
            this.provider.FromXmlString(pubKey);

            this.rsaParams = this.provider.ExportParameters(false);
            MAX_INPUT_BYTES = ((this.provider.KeySize - 384) / 8) + 6;

            this.bFormatter = new BinaryFormatter();

#if DEBUG
            runningInstances++;
#endif
        }

		/// <summary>
		/// Initializes a new instance of RSAHelper with it's values set to the defaults
		/// </summary>
        public RSAHelper(ILogger logger)
        {
            this.logger = logger;
            this.InitNew();
        }

        /// <summary>
        /// Initializes a new instance of RSAHelper with it's values set to the defaults
        /// </summary>
        public RSAHelper()
        {
            this.InitNew();
        }

        private void InitNew()
        {
            CspParameters parameters = GenKeys();

            base.LogInformation("Init");
            this.provider = new RSACryptoServiceProvider(parameters);

            this.rsaParams = this.provider.ExportParameters(false);
            base.LogInformation("Initialized, Modulus Size: {0}B", rsaParams.Modulus.Length);
            MAX_INPUT_BYTES = ((this.provider.KeySize - 384) / 8) + 6;

            this.bFormatter = new BinaryFormatter();
#if DEBUG
            runningInstances++;
#endif
        }

		/// <summary>
		/// Encrypts the specified data using the public key contained withing this RSAHelper, encodes it as a base64 string and returns said string.
		/// </summary>
		/// <param name="data">The data to encrypt</param>
		/// <returns>Returns the encrypted data as a base64 string</returns>
		public string EncryptToBase64String(byte[] data)
		{
            MemoryStream dataStream = new MemoryStream(data);
            using (MemoryStream outputStream = new MemoryStream(rsaParams.Modulus.Length * ((int)Math.Ceiling((double)data.Length / MAX_INPUT_BYTES))))
            {
                byte[] dataSlice = new byte[MAX_INPUT_BYTES];
                int read;
                while ((read = dataStream.Read(dataSlice, 0, MAX_INPUT_BYTES)) > 0)
                {
                    if (read < MAX_INPUT_BYTES)
                    {
                        byte[] temp = new byte[read];
                        Array.Copy(dataSlice, temp, read);
                        dataSlice = temp;
                    }
                    byte[] encryptedSlice = provider.Encrypt(dataSlice, true);
                    outputStream.Write(encryptedSlice, 0, encryptedSlice.Length);
                    dataSlice = new byte[MAX_INPUT_BYTES];
                }
                byte[] ret = outputStream.GetBuffer();
                dataStream.Dispose();
                return Convert.ToBase64String(ret);
            }
		}

		/// <summary>
		/// Decodes a base64 string and decrypts the resulting data using the private key contained in this instance
		/// </summary>
		/// <param name="data">The data to decrypt</param>
		/// <returns>Returns the resulting byte[] (raw)</returns>
		public byte[] DecryptBase64String(string data)
		{
			byte[] p_Data = Convert.FromBase64String(data);
            MemoryStream dataStream = new MemoryStream(p_Data);
            MemoryStream outputStream = new MemoryStream(MAX_INPUT_BYTES * (int)(Math.Ceiling((double)p_Data.Length / rsaParams.Modulus.Length)));
            byte[] buffer = new byte[rsaParams.Modulus.Length];
            int read;
            int total = 0;
            while ((read = dataStream.Read(buffer, 0, rsaParams.Modulus.Length)) == rsaParams.Modulus.Length)
            {
                byte[] decryptedSlice = provider.Decrypt(buffer, true);
                if (decryptedSlice.Length != MAX_INPUT_BYTES)
                {
                    p_Data = outputStream.GetBuffer();
                    byte[] final = new byte[total + decryptedSlice.Length];
                    Array.Copy(p_Data, final, total);
                    Array.Copy(decryptedSlice, 0, final, total, decryptedSlice.Length);
                    return final;
                }
                outputStream.Write(decryptedSlice, 0, decryptedSlice.Length);
                buffer = new byte[rsaParams.Modulus.Length];
                total += decryptedSlice.Length;
            }
            p_Data = outputStream.GetBuffer();
			return p_Data;
		}

		/// <summary>
		/// Gets the public key contained within this instance
		/// Sets the publick key (and any other parameters contained in the value) contained within this instance
		/// </summary>
        public string PublicKey
        {
            get { return provider.ToXmlString(false); }
            set { provider.FromXmlString(value); }
        }

		/// <summary>
		/// Simply deserializes the specified byte[] into an object
		/// </summary>
		/// <param name="data">The data to deserialize</param>
		/// <returns>Returns the resulting object</returns>
        public object Deserialize(byte[] data)
        {
            this.mStream = new MemoryStream(data);
            object result = this.bFormatter.Deserialize(mStream);
            mStream.Flush();
            mStream.Close();
            return result;
        }

		/// <summary>
		/// Simply serializes the specified object using the formatter contained within this class
		/// </summary>
		/// <param name="data">The object to serialize</param>
		/// <returns>Returns the resulting byte[]</returns>
        public byte[] Serialize(object data)
        {
            this.mStream = new MemoryStream();
            this.bFormatter.Serialize(mStream, data);
            this.mStream.Flush();
            byte[] result = this.mStream.GetBuffer();
            this.mStream.Close();
            return result;
        }

        #region IDisposable Members

		/// <summary>
		/// Disposes the RSAEncryptionProvider contained within this instance
		/// </summary>
        public void Dispose()
        {
            provider.Clear();
            provider = null;
            GC.Collect();
        }

        #endregion

        /// <summary>
        /// Gets and Sets the logger used by this instance of RSAHelper
        /// </summary>
        protected override ILogger Logger
        {
            get
            {
                return this.logger;
            }
            set
            {
                this.logger = value;
            }
        }
    }

}