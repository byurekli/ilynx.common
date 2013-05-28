using System;

namespace T0yK4T.Cryptography
{
	/// <summary>
	/// Contains a few helper methods to help hashing passwords or data
	/// </summary>
	public static class HashHelper
	{
		/// <summary>
		/// Hashes the specified string value and returns a base64 encoded byte[] that is the hash
		/// </summary>
		/// <param name="pass">The "password" to hash</param>
		/// <returns>returns a base64 encoded byte[] that is the hash</returns>
		public static string Hash(string pass)
		{
			return Hash(CryptoCommon.ENCODING.GetBytes(pass));
		}

		/// <summary>
		/// Hashes the specified, arbitrary amount, of data and returns a base64 encoded byte[] that is the hash
		/// </summary>
		/// <param name="data">The data to hash</param>
		/// <returns>returns a base64 encoded byte[] that is the hash</returns>
		public static string Hash(byte[] data)
		{
			byte[] hash = CryptoCommon.Hasher.ComputeHash(data);
			return Convert.ToBase64String(hash);
		}
	}
}