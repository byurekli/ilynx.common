using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ProtoBuf;

namespace T0yK4T.IO
{
    /// <summary>
    /// Used for accessing the ProtoBuf serializer
    /// </summary>
    public static class ToySerializer
    {
        //private static readonly object serializerLock = new object();
        //private static readonly object deserializerLock = new object();

        /// <summary>
        /// Attempts to serialize the specified object in to a byte array
        /// <para/>
        /// This method embeds a length field in to the final result (which is then used by <see cref="DeserializeWithLength"/> upon deserialization)
        /// <para/>
        /// returns a zero length array if unsuccesful
        /// </summary>
        /// <typeparam name="T">The type to serialize</typeparam>
        /// <param name="type">The instance of <typeparamref name="T"/> to serialize</param>
        /// <returns></returns>
        public static byte[] SerializeWithLength<T>(T type) where T : class, new()
        {
            if (type == null)
                return new byte[0];
            try
            {
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
            catch { return new byte[0]; }
        }

        /// <summary>
        /// Attempts to serialize the specified object in to a byte array
        /// <para/>
        /// returns a zero length array if unsuccesful
        /// </summary>
        /// <typeparam name="T">The type to serialize</typeparam>
        /// <param name="data">The instance of <typeparamref name="T"/> to serialize</param>
        /// <returns></returns>
        public static byte[] Serialize<T>(T data) where T : class, new()
        {
            if (data == null)
                return new byte[0];
            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    Serializer.Serialize(stream, data);
                    byte[] ret = new byte[stream.Length];
                    stream.Position = 0;
                    byte[] streamData = stream.GetBuffer();
                    Array.Copy(streamData, 0, ret, 0, ret.Length);
                    return ret;
                }
            }
            catch { return new byte[0]; }
        }

        /// <summary>
        /// Attempts to deserialize the specified data in to a new instance of <typeparamref name="T"/>
        /// <para/>
        /// This method expects a length field to be embedded in the input data
        /// <para/>
        /// Returns null if unsuccesful
        /// </summary>
        /// <typeparam name="T">The type to deserialize in to</typeparam>
        /// <param name="data">The byte array to deserialize</param>
        /// <returns></returns>
        public static T DeserializeWithLength<T>(byte[] data) where T : class, new()
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

        /// <summary>
        /// Attempts to deserialize the specified data in to a new instance of <typeparamref name="T"/>
        /// <para/>
        /// Returns null if unsuccesful
        /// </summary>
        /// <typeparam name="T">The type to deserialize in to</typeparam>
        /// <param name="data">The byte array to deserialize</param>
        /// <returns></returns>
        public static T Deserialize<T>(byte[] data) where T : class, new()
        {
            try
            {
                T ret;
                using (MemoryStream memStream = new MemoryStream(data))
                    ret = Serializer.Deserialize<T>(memStream);
                return ret;
            }
            catch { return null; }
        }
    }
}
