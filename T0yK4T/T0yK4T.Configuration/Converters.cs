using System;
using System.Net;

namespace T0yK4T.Configuration
{
    /// <summary>
    /// Basic String Converter (Well, no conversion is actuall performed in this case...
    /// </summary>
    public class StringConverter : IValueConverter<string>
    {
        /// <summary>
        /// ...
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string Convert(string value)
        {
            return value;
        }

        /// <summary>
        /// ...
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string ToString(string value)
        {
            return value;
        }
    }

    /// <summary>
    /// Basic boolean converter
    /// </summary>
    public class BooleanConverter : IValueConverter<bool>
    {
        /// <summary>
        /// Returns the output of <see cref="bool.TryParse"/> (if succesful)
        /// <para/>
        /// otherwise throws a <see cref="System.FormatException"/>
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Convert(string value)
        {
            bool val;
            if (bool.TryParse(value, out val))
                return val;
            else
                throw new FormatException("the given string value was in an incorrect format");
        }

        /// <summary>
        /// Returns <see cref="bool.ToString()"/>
        /// </summary>
        /// <param name="value">The value to get a string representation of</param>
        /// <returns><see cref="bool.ToString()"/></returns>
        public string ToString(bool value)
        {
            return value.ToString();
        }
    }

    /// <summary>
    /// Basic int32 converter
    /// </summary>
    public class Int32Converter : IValueConverter<int>
    {

        /// <summary>
        /// Attempts to convert the specified value to an integer
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <returns>The integer value represented in <paramref name="value"/> if succesful</returns>
        /// <exception cref="FormatException">Throw if the value cannot be converted</exception>
        public int Convert(string value)
        {
            int val;
            if (int.TryParse(value, out val))
                return val;
            else
                throw new FormatException("the given string value was in an incorrect format");
        }

        /// <summary>
        /// Returns <see cref="int.ToString()"/>
        /// </summary>
        /// <param name="value">The integer value to get a string representation of</param>
        /// <returns>A string representation of the specified value</returns>
        public string ToString(int value)
        {
            return value.ToString();
        }
    }

    /// <summary>
    /// A basic IPAddress converter
    /// </summary>
    public class IPAddressConverter : IValueConverter<IPAddress>
    {
        /// <summary>
        /// Attempts to convert the specified string to an IPAddress
        /// </summary>
        /// <param name="value">The string to convert</param>
        /// <returns>The IPAddress represented by <paramref name="value"/></returns>
        public IPAddress Convert(string value)
        {
            IPAddress val = IPAddress.None;
            if (IPAddress.TryParse(value, out val))
                return val;
            else
                throw new FormatException("the given string value was in an incorrect format");
        }

        /// <summary>
        /// Returns <see cref="IPAddress.ToString()"/>
        /// </summary>
        /// <param name="value">The value to convert to a string</param>
        /// <returns>The string representation of <paramref name="value"/></returns>
        public string ToString(IPAddress value)
        {
            return (value ?? IPAddress.Any).ToString();
        }
    }

    /// <summary>
    /// Basic UInt16 converter...
    /// </summary>
    public class UInt16Converter : IValueConverter<ushort>
    {
        /// <summary>
        /// Attempts to convert the specified string value to an unsigned 16 bit integer
        /// </summary>
        /// <param name="value">The string representation to convert (parse)</param>
        /// <returns>The value represented by the string specified, otherwise throws a <see cref="FormatException"/></returns>
        /// <exception cref="FormatException">Thrown if <paramref name="value"/> is not a recognizable ushort (unsigned 16 bit integer)</exception>
        public ushort Convert(string value)
        {
            ushort val;
            if (ushort.TryParse(value, out val))
                return val;
            throw new FormatException("the given string value was in an incorrect format");
        }

        /// <summary>
        /// Returns a basic string representation of the specified value
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <returns><see cref="ushort.ToString()"/></returns>
        public string ToString(ushort value)
        {
            return value.ToString();
        }
    }
}