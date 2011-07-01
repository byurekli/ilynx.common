using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using T0yK4T.Tools.Cryptography;

namespace T0yK4T.Tools.Configuration
{
	/// <summary>
	/// Can help manage configurations securely (IE. encrypt / decrypt the contents of configuration files)
	/// This is still in the experimental stages - use at your own risk
	/// </summary>
    public class SafeConfigManager
    {
        private Dictionary<string, string> rawData = new Dictionary<string, string>();
        private string fileName;
        EncryptionProvider ecp;

		/// <summary>
		/// Initializes a new instance of SafeConfigManager
		/// </summary>
		/// <param name="fileName">The full filename (Path + Filename) of the configuration file to open</param>
		/// <param name="key">The Key used to encrypt and decrypt configuration values</param>
        public SafeConfigManager(string fileName, byte[] key)
        {
            this.ecp = new EncryptionProvider(key, key, new BinaryFormatter(), true);
			this.Initialize(fileName);
        }

		/*public SafeConfigManager(string fileName)
		{
			this.ecp = new EncryptionProvider(Constants.MACHINE_Key);
			this.Initialize(fileName);
		}*/

		private void Initialize(string fileName)
		{
			this.fileName = fileName;
			if (File.Exists(fileName))
				this.Open(fileName);
		}

		/// <summary>
		/// Opens a configuration file
		/// </summary>
		/// <param name="fileName">The full filename (Path + Filename) of the configuration file to open</param>
        public void Open(string fileName)
        {
            if (!File.Exists(fileName))
                throw new ArgumentException("File " + fileName + " Does not exist!");
            this.fileName = fileName;
            FileStream fStream = File.Open(fileName, FileMode.Open, FileAccess.Read);
            using (StreamReader reader = new StreamReader(fStream))
            {
                string data;
                while ((data = reader.ReadLine()) != null)
                {
					try
					{
						string[] split = data.Split(CryptoCommon.TYPE_DATA_SEPERATOR_CHAR);
						this.rawData.Add(split[0], split[1]);
					}
					catch { continue; }
                }
                reader.Close();
            }
            fStream.Close();
        }

		/// <summary>
		/// Attempts to get a value of the specified type
		/// </summary>
		/// <typeparam name="T">The type to search for</typeparam>
		/// <param name="res">The resulting value will be put here if it was found</param>
		/// <returns>Returns true if the specified key was found, otherwise false</returns>
        public bool GetValue<T>(out T res)
        {
			res = default(T);
			try
			{
				if (this.rawData.ContainsKey(typeof(T).ToString()))
				{
					res = (T)this.ecp.Decrypt(Convert.FromBase64String(this.rawData[typeof(T).ToString()]));
					//this.rawData.Remove(typeof(T).ToString());
					return true;
				}
				else
					return false;
			}
			catch { return false; }
        }

		/// <summary>
		/// Pushes or Sets a value in the configuration file 
		/// If the typekey (Type.ToString()) does not exist in the configuration yet, it will be added
		/// If the typekey DOES exist, the value will be set to the specified object
		/// </summary>
		/// <param name="value">The value to push</param>
        public void PushOrSetValue(object value)
        {
            if (!this.rawData.ContainsKey(value.GetType().ToString()))
                this.rawData.Add(value.GetType().ToString(), Convert.ToBase64String(this.ecp.Encrypt(value)));
            else
                this.rawData[value.GetType().ToString()] = Convert.ToBase64String(this.ecp.Encrypt(value));
        }

		/// <summary>
		/// Writes the entire shabang to disk
		/// </summary>
        public void Save()
        {
            if (File.Exists(this.fileName))
                File.Delete(this.fileName);
            FileStream fStream = File.Open(this.fileName, FileMode.CreateNew, FileAccess.Write);
            using (StreamWriter writer = new StreamWriter(fStream))
            {
				foreach (string type in this.rawData.Keys)
					writer.WriteLine(type + CryptoCommon.TYPE_DATA_SEPERATOR_CHAR.ToString() + this.rawData[type]);
                writer.Close();
            }
            fStream.Close();
        }

		/// <summary>
		/// Writes the configuration file to the specified location, optionally overwrites any existing files
		/// </summary>
		/// <param name="fileName">The full filename to save as</param>
		/// <param name="overWrite">If set to true, will overwrite existing files, if false, will fail silently</param>
        public void SaveAs(string fileName, bool overWrite)
        {
			if (!overWrite && File.Exists(fileName))
				return;
			string orig = this.fileName;
            this.fileName = fileName;
            this.Save();
			this.fileName = orig;
        }
    }
}
