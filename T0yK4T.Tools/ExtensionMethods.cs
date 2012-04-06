

using System.Collections.Generic;
namespace T0yK4T.Tools
{
    public static class ExtensionMethods
    {
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