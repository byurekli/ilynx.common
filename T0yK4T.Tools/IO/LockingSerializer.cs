using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ProtoBuf;

namespace T0yK4T.Tools.IO
{
    /// <summary>
    /// Used for accessing the ProtoBuf serializer
    /// </summary>
    public static class ToySerializer
    {
        private static readonly object serializerLock = new object();
        private static readonly object deserializerLock = new object();

        /// <summary>
        /// Locks the internal serializer object and serializes the specified type instance
        /// </summary>
        /// <typeparam name="T">The type to serialize</typeparam>
        /// <param name="type">The instance of {T} to serialize</param>
        /// <returns></returns>
        public static byte[] Serialize<T>(T type) where T : class, new()
        {
            if (type == null)
                return new byte[0];
            using (MemoryStream outputStream = new MemoryStream(1))
            {
                Serializer.Serialize<T>(outputStream, type);
                byte[] data = outputStream.GetBuffer();
                byte[] actualData = new byte[outputStream.Length + sizeof(long)];
                byte[] lengthField = BitConverter.GetBytes(outputStream.Length);
                Array.Copy(lengthField, actualData, lengthField.Length);
                Array.Copy(data, 0, actualData, lengthField.Length, outputStream.Length);
                return actualData;
            }
        }

        /// <summary>
        /// Locks the internal deserializer object and deserializes the specified byte array in to the specified type
        /// </summary>
        /// <typeparam name="T">The type to deserialize in to</typeparam>
        /// <param name="data">The byte array to deserialize</param>
        /// <returns></returns>
        public static T Deserialize<T>(byte[] data) where T : class, new()
        {
            try
            {
                long length = BitConverter.ToInt64(data, 0);
                byte[] actualData = new byte[length];
                Array.Copy(data, sizeof(long), actualData, 0, actualData.LongLength);
                using (MemoryStream stream = new MemoryStream(actualData))
                {
                    T ret;
                    ret = Serializer.Deserialize<T>(stream);
                    return ret;
                }
            }
            catch { return null; }
        }
    }
}
