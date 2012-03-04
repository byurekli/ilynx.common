using System.Security.Cryptography;
using System;
using SkeinFish;
using System.Text;

namespace T0yK4T.Tools.Cryptography
{
	/// <summary>
	/// Contains commonly used properties and values for encryption (Also contains  the default values for the encryption provider's default constructor)
	/// </summary>
    public static class CryptoCommon
    {
        /// <summary>
        /// The default key size (in bits) used by the Hasher
        /// </summary>
        public const uint KEY_SIZE = 1024;

        /// <summary>
        /// The character used to seperate data (This is used in the SafeConfigurationManager to distinguish between Type (Key) and Data (Value))
        /// </summary>
        public const char TYPE_DATA_SEPERATOR_CHAR = ' ';

        /// <summary>
        /// Indicates the default value for compression (Will be used by EncryptionProvider as a default value)
        /// </summary>
        public const bool COMPRESS = false;

        /// <summary>
        /// The default cipher mode used in various classes
        /// </summary>
        public static CipherMode CipherMode = CipherMode.CBC;

        /// <summary>
        /// The default Encoding used to convert strings to byte[] and vice versa (Will be used by EncryptionProvider as a default value)
        /// </summary>
        public static readonly Encoding ENCODING = ASCIIEncoding.ASCII;

        private static Skein hasher;

        /// <summary>
        /// Returns a SkeinFish.Skein to compute hashes with
        /// </summary>
        public static Skein Hasher
        {
            get
            {
                if (hasher == null)
                {
                    hasher = new Skein((int)KEY_SIZE, (int)KEY_SIZE);
                    hasher.UbiParameters.BlockType = UbiType.Key;
                }
                return hasher;
            }
        }

        /// <summary>
        /// Contains the default formatter to use when serializing and deserializing data (Will be used by EncryptionProvider as a default value)
        /// </summary>
        public static readonly System.Runtime.Serialization.IFormatter DEFAULT_FORMATTER = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
        //public static readonly byte[] MACHINE_Key = EncryptionProvider.Hash(BIOS_Reader.GetMachineID(), ENCODING);

        private static System.Security.Cryptography.RandomNumberGenerator prng = System.Security.Cryptography.RandomNumberGenerator.Create();

        /// <summary>
        /// Default instance of a <see cref="System.Security.Cryptography.RandomNumberGenerator"/>
        /// </summary>
        public static System.Security.Cryptography.RandomNumberGenerator Prng
        {
            get { return prng; }
        }

        /// <summary>
        /// Gets the next available random integer using a Pseudo<see cref="RandomNumberGenerator"/> to generate random bytes
        /// </summary>
        /// <returns></returns>
        public static int GetPrngInt()
        {
            byte[] buffer = new byte[System.Runtime.InteropServices.Marshal.SizeOf(typeof(int))];
            byte[] buffer2 = new byte[buffer.Length];
            prng.GetNonZeroBytes(buffer);
            prng.GetBytes(buffer2);
            for (int i = 0; i < buffer.Length; i++)
                buffer[i] ^= buffer2[(buffer.Length - 1) - i];
            return System.BitConverter.ToInt32(buffer, 0);
        }
    }
}