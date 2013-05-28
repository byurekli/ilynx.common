

using System.Collections.Generic;
namespace T0yK4T.Tools
{
    /// <summary>
    /// A few general Extension Methods
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Gets a string representation of the specified <see name="IEnumerable{byte}"/> using the specified <paramref name="splitter"/> as a "splitter"
        /// </summary>
        /// <param name="val">The <see cref="IEnumerable{T}"/> to stringify</param>
        /// <param name="splitter">The splitter to use between bytes</param>
        /// <returns></returns>
        public static string ToString(this IEnumerable<byte> val, string splitter)
        {
            string ret = string.Empty;
            foreach (byte v in val)
                ret += v.ToString("X2") + splitter;
            if (ret.EndsWith(splitter))
                ret = ret.Remove(ret.LastIndexOf(splitter), splitter.Length);
            return ret;
        }
    }
}