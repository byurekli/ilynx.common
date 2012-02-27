using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using SkeinFish;

namespace T0yK4T.Tools.Cryptography
{
	/// <summary>
	/// Contains helper methods for encryption and decryption + serialization and deserialization
	/// </summary>
	public class EncryptionProvider : IDisposable
	{
		private MemoryStream dataStream;
		private object p_Lock = new object();
		private SymmetricAlgorithm symAlg;
        private bool compress = false;
        private GZipStream gStream;
		private ICryptoTransform encryptor;
		private ICryptoTransform decryptor;
        private RandomNumberGenerator prng = RandomNumberGenerator.Create();

		/// <summary>
		/// Initializes a new instance of EncryptionProvider with Key and IV set to the specified values
        /// <para/>
        /// NOTE: Key and IV will be randomized after this constructor has run
		/// </summary>
		/// <param name="formatter">The Serialization Formatter to use when serializing objects</param>
		/// <param name="compress">Specifies whether or not Compression and Decompression should occur before Encryption and after Decryption</param>
		/// <param name="iv">The Initialization Vector to use (This will only be used if the ciphermode (such as CBC) requires it!)</param>
		/// <param name="key">The Key to use in the encryption algorithm</param>
		public EncryptionProvider(byte[] key, byte[] iv, IFormatter formatter, bool compress)
		{
            this.compress = compress;
			this.Initialize(key, iv);
            prng.GetBytes(key);
            prng.GetBytes(iv);
		}

        /// <summary>
        /// Initializes a new instance of EncryptionProvider and sets the key and initialization vector to the specified values
        /// <para/>
        /// The internal dataformatter for serialization is set to <see cref="CryptoCommon.DEFAULT_FORMATTER"/>
        /// <para/>
        /// The "compress flag" is set to <see cref="CryptoCommon.COMPRESS"/>
        /// <para/>
        /// NOTE: Key and IV will be randomized after this constructor has run
        /// </summary>
        /// <param name="key">The key to use for the underlying symetric encryption algorithm</param>
        /// <param name="iv">The initlaization vector to use for the underlying encryption algorithm</param>
        public EncryptionProvider(byte[] key, byte[] iv)
        {
            this.compress = CryptoCommon.COMPRESS;
            this.Initialize(key, iv);
            prng.GetBytes(key);
            prng.GetBytes(iv);
        }

		/// <summary>
		/// Initializes a new instance of EncryptionProvider with all it's values set to the defaults - except the compress "flag" (Key and IV will be RANDOM)
		/// </summary>
		/// <param name="compress">Specifies wether or not compression / decompression should occur before encryption and after decryption</param>
		public EncryptionProvider(bool compress)
		{
			this.compress = compress;
            byte[] key = new byte[CryptoCommon.KEY_SIZE / 8];
            byte[] iv = new byte[CryptoCommon.KEY_SIZE / 8];
            prng.GetBytes(key);
            prng.GetBytes(iv);

            this.Initialize(key, iv);
		}

        /// <summary>
        /// Gets the BlockSize of the current encryption algorithm
        /// </summary>
        public int WriteBlockSize
        {
            get { return this.symAlg.BlockSize / 8; }
        }

        /// <summary>
        /// Gets an instance of <see cref="ICryptoTransform"/> that can be used to encrypt data
        /// </summary>
        public ICryptoTransform Encryptor
        {
            get { return this.encryptor; }
        }

        /// <summary>
        /// Gets an instance of <see cref="ICryptoTransform"/> that can be used to decrypt data
        /// </summary>
        public ICryptoTransform Decryptor
        {
            get { return this.decryptor; }
        }

		/// <summary>
		/// Initializes a new instance of EncryptionProvider with it's values set to all defaults (Key and IV will be RANDOM!)
		/// </summary>
		public EncryptionProvider()
			: this(CryptoCommon.COMPRESS) { }

        /// <summary>
        /// Kills any and all key data used
        /// </summary>
		~EncryptionProvider()
		{
			this.symAlg.GenerateKey();
			this.symAlg.GenerateIV();
			this.symAlg.Clear();
		}

		/// <summary>
		/// Resets the encryption provider ... tbc
		/// </summary>
		public void Reset()
		{
            this.symAlg.Mode = CryptoCommon.CipherMode;
			this.encryptor = this.symAlg.CreateEncryptor();
			this.decryptor = this.symAlg.CreateDecryptor();
		}

		private void Initialize(byte[] key, byte[] iv)
		{
			symAlg = new Threefish();
			symAlg.KeySize = key.Length * 8;
			symAlg.BlockSize = key.Length * 8;
			symAlg.Mode = CryptoCommon.CipherMode;
            symAlg.Padding = PaddingMode.None;
            if (key == null)
                symAlg.GenerateKey();
            else
			    symAlg.Key = key;
            
            if (iv == null)
                symAlg.GenerateIV();
            else
			    symAlg.IV = iv;
			this.encryptor = this.symAlg.CreateEncryptor();
			this.decryptor = this.symAlg.CreateDecryptor();
			/*symAlg = new RijndaelManaged();
			symAlg.KeySize = 256;
			symAlg.BlockSize = 256;
			symAlg.Key = key.Take(256 / 8).ToArray();
			symAlg.IV = iv.Take(256 / 8).ToArray();*/
		}

        ///// <summary>
        ///// Serializes and Encrypts an object
        ///// </summary>
        ///// <param name="data"></param>
        ///// <returns></returns>
        //public byte[] Encrypt<T>(T data)
        //{
        //    if (!data.GetType().IsDefined(typeof(SerializableAttribute), false))
        //        return new byte[0];
        //    return this.EncryptArray(this.Serialize<T>(data));
        //}

        ///// <summary>
        ///// Decrypts an array of data using the Cryptographic Algorithm contained within this class and deserializes it in to an object
        ///// </summary>
        ///// <param name="data"></param>
        ///// <returns></returns>
        //public object Decrypt(byte[] data)
        //{
        //    return this.Deserialize(this.DecryptArray(data));
        //}

        ///// <summary>
        ///// Serializes specified (payload) - T
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="data"></param>
        ///// <returns></returns>
        //public byte[] Serialize<T>(T data)
        //{
        //    if (!this.CanSerialize<T>())
        //        return new byte[0];
        //    byte[] iData;
        //    lock (this.p_Lock)
        //    {
        //        try
        //        {
        //            this.dataStream = new MemoryStream();
        //            if (this.compress)
        //            {
        //                this.gStream = new GZipStream(dataStream, CompressionMode.Compress);
        //                this.bFormatter.Serialize(this.gStream, data);
        //                this.gStream.Flush();
        //            }
        //            else
        //                this.bFormatter.Serialize(dataStream, data);
        //            //this.dataStream.Flush();
        //            iData = this.dataStream.GetBuffer();
        //            this.dataStream.Close();
        //        }
        //        catch
        //        {
        //            try
        //            {
        //                if (this.compress)
        //                    this.gStream.Close();
        //                this.dataStream.Close();
        //            }
        //            catch { }
        //            iData = new byte[0];
        //        }
        //    }
        //    return iData;
        //}

		/// <summary>
		/// Compresses the specified array of data using the builtin GZip compression
		/// </summary>
		/// <param name="data">The data to compress</param>
		/// <returns>Returns the resulting byte[]</returns>
		public byte[] Compress(byte[] data)
		{
            return this.DoCompressDecompress(data, CompressionMode.Compress);
		}

        /// <summary>
        /// Decompresses the specified byte[]
        /// </summary>
        /// <param name="data">The data to decompress</param>
        /// <returns>Returns the resulting byte[]</returns>
        public byte[] Decompress(byte[] data)
        {
            return this.DoCompressDecompress(data, CompressionMode.Decompress);
        }

        private byte[] DoCompressDecompress(byte[] data, CompressionMode mode)
        {
            byte[] res = new byte[0];
            lock (this.p_Lock)
            {
                this.dataStream = new MemoryStream();
                this.gStream = new GZipStream(this.dataStream, mode);
                try
                {
                    this.gStream.Write(data, 0, data.Length);
                    this.gStream.Flush();
                    res = this.dataStream.GetBuffer();
                    this.dataStream.Close();
                }
                catch { this.dataStream.Close(); }
            }
            return res;
        }

        ///// <summary>
        ///// Deserializes the specified array of bytes into an object
        ///// if Compress is true, this method will decompress the data before deserialization!
        ///// </summary>
        ///// <param name="data"></param>
        ///// <returns></returns>
        //public object Deserialize(byte[] data)
        //{
        //    object retVal = default(object);
        //    try
        //    {
        //        lock (this.p_Lock)
        //        {
        //            this.dataStream = new MemoryStream(data);
        //            if (this.compress)
        //            {
        //                this.gStream = new GZipStream(this.dataStream, CompressionMode.Decompress);
        //                retVal = this.bFormatter.Deserialize(this.gStream);
        //                this.gStream.Close();
        //            }
        //            else
        //                retVal = this.bFormatter.Deserialize(this.dataStream);
        //            this.dataStream.Close();
        //        }
        //    }
        //    catch { this.dataStream.Close(); }
        //    return retVal;
        //}

        ///// <summary>
        ///// Deserializes the specified stream and returns the resulting object, optionally closes the stream after reading is done
        ///// </summary>
        ///// <param name="s">The stream to deserialize data from</param>
        ///// <param name="closeStream">If true, will close the specified stream after deserialization, otherwise false</param>
        ///// <returns>Returns the resulting object</returns>
        //public object Deserialize(Stream s, bool closeStream)
        //{
        //    object retVal = default(object);
        //    try
        //    {
        //        lock (this.p_Lock)
        //        {
        //            retVal = this.bFormatter.Deserialize(s);
        //            if (closeStream)
        //                s.Close();
        //        }
        //    }
        //    catch
        //    {
        //        if (closeStream)
        //            s.Close();
        //    }
        //    return retVal;
        //}

        ///// <summary>
        ///// Serializes the specified object onto the specified stream, and optionally closes the stream on completion
        ///// </summary>
        ///// <param name="o">The object to serialize</param>
        ///// <param name="s">The stream to use for serialization output</param>
        ///// <param name="closeStream">If set to true, will close the specified stream, otherwise won't</param>
        //public void Serialize(object o, Stream s, bool closeStream)
        //{
        //    if (!this.CanSerialize(o))
        //        throw new ArgumentException("Specified object is not serializable!");
        //    try
        //    {
        //        this.bFormatter.Serialize(s, o);
        //        s.Flush();
        //        if (closeStream)
        //            s.Close();
        //    }
        //    catch
        //    {
        //        if (closeStream)
        //            s.Close();
        //    }
        //}

        //private bool CanSerialize(object o)
        //{
        //    return o.GetType().IsDefined(typeof(SerializableAttribute), false);
        //}

        //private bool CanSerialize<T>()
        //{
        //    return typeof(T).IsDefined(typeof(SerializableAttribute), false);
        //}


		/// <summary>
		/// "Transforms" the specified byte[] using the specified ICyptoTransform (This method can be used to encrypt and decrypt data easily)
		/// </summary>
		/// <param name="transform">The ICryptoTransform to use</param>
		/// <param name="data">The data to "Transform"</param>
		/// <returns>Returns the resulting byte[]</returns>
        public static byte[] Transform(ICryptoTransform transform, byte[] data)
        {
            MemoryStream memStream = new MemoryStream();
            CryptoStream cStream = new CryptoStream(memStream, transform, CryptoStreamMode.Write);
            cStream.Write(data, 0, data.Length);
            cStream.Flush();
            cStream.FlushFinalBlock();
            memStream.Position = 0;
            byte[] enc = memStream.GetBuffer();
            cStream.Close();
            memStream.Close();
            return enc;
        }

		/// <summary>
		/// Compresses (if enabled) and Encrypts an array of bytes using the internal <see cref="ICryptoTransform"/>
        /// <para/>
        /// this method will use the transform available through <see cref="EncryptionProvider.Encryptor"/>
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public byte[] EncryptArray(byte[] data)
		{
            if (this.compress)
                data = this.Compress(data);
            return EncryptArray(data, this.encryptor);
		}

        /// <summary>
        /// Decrypts and Decompresses (if enabled) an aray of bytes using the internal <see cref="ICryptoTransform"/>
        /// <para/>
        /// this method will use the transform available through <see cref="EncryptionProvider.Decryptor"/>
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public byte[] DecryptArray(byte[] data)
        {
            byte[] res = DecryptArray(data, this.decryptor);
            if (this.compress)
                res = this.Decompress(res);
            return res;
        }

        /// <summary>
        /// For internal use (this decrypts an array of data and outputs the correct length data)
        /// </summary>
        /// <param name="data">The data to encrypt</param>
        /// <param name="transform"></param>
        /// <returns></returns>
        private static byte[] DecryptArray(byte[] data, ICryptoTransform transform)
        {
            using (MemoryStream memStream = new MemoryStream(data))
            {
                using (CryptoStream stream = new CryptoStream(memStream, transform, CryptoStreamMode.Read))
                {
                    byte[] lengthField = new byte[sizeof(int)];
                    stream.Read(lengthField, 0, lengthField.Length);
                    int length = BitConverter.ToInt32(lengthField, 0);
                    byte[] actualData = new byte[length];
                    int read = stream.Read(actualData, 0, actualData.Length);
                    if (read != length)
                        Console.WriteLine("Unexpected length");
                    return actualData;
                }
            }
        }


        /// <summary>
        /// For internal use (this pads (if necessary) and encrypts an array of data, and includes a length field in the final encrypted data (this field (4 bytes) is encrypted as well))
        /// </summary>
        /// <param name="data">The data to encrypt</param>
        /// <param name="transform">The <see cref="ICryptoTransform"/> to use</param>
        /// <returns></returns>
        private static byte[] EncryptArray(byte[] data, ICryptoTransform transform)
        {
            using (MemoryStream memStream = new MemoryStream())
            {
                using (CryptoStream stream = new CryptoStream(memStream, transform, CryptoStreamMode.Write))
                {
                    int bytes = transform.InputBlockSize;
                    long rem = (bytes - (data.Length % bytes)) - sizeof(int);
                    byte[] newData = new byte[data.Length + rem + sizeof(int)];
                    byte[] rnd = new byte[rem];
                    CryptoCommon.Prng.GetBytes(rnd);
                    Array.Copy(data, 0, newData, sizeof(int), data.Length);
                    Array.Copy(rnd, 0, newData, data.Length + sizeof(int), rnd.Length);
                    byte[] lengthField = BitConverter.GetBytes(data.Length);
                    Array.Copy(lengthField, 0, newData, 0, lengthField.Length);
                    stream.Write(newData, 0, newData.Length);
                    memStream.Position = 0;
                    byte[] result = memStream.GetBuffer();
                    byte[] final = new byte[memStream.Length];
                    Array.Copy(result, 0, final, 0, final.Length);
                    return final;
                }
            }
        }

		/// <summary>
		/// Sets the password used to encrypt and decrypt data in this instance of EncryptionProvider
		/// NOTE: This replaces the IV aswell!!!
		/// </summary>
		public string Password
		{
			set
			{
				byte[] bKey = Hash(value, CryptoCommon.ENCODING);
				this.Initialize(bKey, bKey);
			}
		}

		/// <summary>
		/// Simply computes the hash of the specified string (represented as a byte[] using the specified text encoder)
		/// </summary>
		/// <param name="hashThis">The string to compute the hash from</param>
		/// <param name="encoder">The encoder to use when "converting" (encoding) the string as a byte[]</param>
		/// <returns></returns>
		public static byte[] Hash(string hashThis, Encoding encoder)
		{
			try
			{
				return CryptoCommon.Hasher.ComputeHash(encoder.GetBytes(hashThis));
			}
			catch
			{ return new byte[0]; }
		}

		/// <summary>
		/// Simply hashes the specified byte[]
		/// </summary>
		/// <param name="hashThis">The data to compute a hash from</param>
		/// <returns>Returns the resulting hash as a byte[]</returns>
		public static byte[] Hash(byte[] hashThis)
		{
			try
			{
				return CryptoCommon.Hasher.ComputeHash(hashThis);
			}
			catch { return new byte[0]; }
		}

		/// <summary>
		/// Hashes and converts a specified string to a hashstring that can be saved as text
		/// </summary>
		/// <param name="hashThis"></param>
		/// <param name="encoder"></param>
		/// <returns></returns>
		public static string HashAndConvert(string hashThis, Encoding encoder)
		{
			try
			{
				byte[] rawHash = CryptoCommon.Hasher.ComputeHash(encoder.GetBytes(hashThis));
				string res = string.Empty;
				foreach (byte b in rawHash)
					res += b.ToString("X4").Remove(0, 2) + ":";
				return res.Remove(res.LastIndexOf(':'));
			}
			catch (Exception er)
			{ Console.WriteLine(er.ToString()); return string.Empty; }
		}

        /// <summary>
        /// Gets or Sets the Key used for encryption and decryption by the encryption algorithm
        /// </summary>
        public byte[] Key
        {
            get { return this.symAlg.Key; }
            set { this.symAlg.Key = value; }
        }

        /// <summary>
        /// Gets or Sets the initialization vector used for encryption by the encryption algorithm
        /// </summary>
        public byte[] IV
        {
            get { return this.symAlg.IV; }
            set { this.symAlg.IV = value; }
        }

		#region IDisposable Members

		/// <summary>
		/// Randomizes the key and initialization vector contained in the symmetric algorithm contained within this instance and clears it
		/// </summary>
		public void Dispose()
		{
			this.symAlg.GenerateKey();
			this.symAlg.GenerateIV();
			this.symAlg.Clear();
		}

		#endregion
	}
}
