using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Net;

namespace T0yK4T.Configuration
{
    /// <summary>
    /// Simple helper class that contains a <see cref="System.Configuration.Configuration"/> object for the entry assembly
    /// </summary>
    public static class Config
    {
        private static System.Configuration.Configuration configuration;

        /// <summary>
        /// Gets a <see cref="System.Configuration.Configuration"/> object for the entry assembly
        /// </summary>
        public static System.Configuration.Configuration Configuration
        {
            get
            {
                if (configuration == null)
                    configuration = ConfigurationManager.OpenExeConfiguration(System.Reflection.Assembly.GetEntryAssembly().Location);
                return configuration;
            }
        }
    }
    /// <summary>
    /// A simple configurable value (Uses the builtin ConfigurationManager (<see cref="System.Configuration.ConfigurationManager"/>)
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class ConfigurableValue<TValue>
    {
        private string name;
        private TValue value;
        private IValueConverter<TValue> converter;

        /// <summary>
        /// Initializes a new instance of <see cref="ConfigurableValue{TValue}"/> and attempts to load it's value from the
        /// <para/>
        /// configuration manager (using the specified <see cref="IValueConverter{TValue}"/> to convert the retrieved string to a <typeparamref name="TValue"/> type
        /// </summary>
        /// <param name="name">The name of the value to look for (Key)</param>
        /// <param name="converter">The <see cref="IValueConverter{TValue}"/> to use for string to <typeparamref name="TValue"/> conversion</param>
        /// <param name="defaultValue">The default value to use if the value could not be retrieved from the configuration file</param>
        public ConfigurableValue(string name, IValueConverter<TValue> converter, TValue defaultValue)
        {
            this.name = name;
            this.converter = converter;
            try
            {
                if (Config.Configuration.AppSettings.Settings.AllKeys.Contains(name))
                {
                    string sVal = Config.Configuration.AppSettings.Settings[this.name].Value;
                    this.value = converter.Convert(sVal);
                }
                else
                {
                    this.value = defaultValue;
                    this.Store();
                }
            }
            catch { this.value = defaultValue; try { this.Store(); } catch { } }
        }

        /// <summary>
        /// Attempts to store this value in the configuration file
        /// </summary>
        public void Store()
        {
            
            if (!Config.Configuration.AppSettings.Settings.AllKeys.Contains(this.name))
                Config.Configuration.AppSettings.Settings.Add(this.name, this.converter.ToString(this.value));
            else
                Config.Configuration.AppSettings.Settings[this.name].Value = this.converter.ToString(this.value);
            try { Config.Configuration.Save(ConfigurationSaveMode.Modified); }
            catch { }
        }

        /// <summary>
        /// Attempts to load this value from file (returns true if succesful, otherwise false)
        /// </summary>
        /// <returns></returns>
        public bool TryLoad()
        {
            try
            {
                string sVal = Config.Configuration.AppSettings.Settings[this.name].Value;
                this.value = this.converter.Convert(sVal);
            }
            catch { return false; }
            return true;
        }

        /// <summary>
        /// Gets or Sets the value of this <see cref="ConfigurableValue{TValue}"/>
        /// </summary>
        public TValue Value
        {
            get { return this.value; }
            set { this.value = value; }
        }

        /// <summary>
        /// Gets or Sets the name of this <see cref="ConfigurableValue{TValue}"/>
        /// </summary>
        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }

        /// <summary>
        /// Simply retrieves the value of the specified <see cref="ConfigurableValue{TValue}"/>
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static implicit operator TValue(ConfigurableValue<TValue> val)
        {
            return val.value;
        }

        /// <summary>
        /// Simply returns <see cref="ConfigurableValue{TValue}.ToString()"/>
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static explicit operator string(ConfigurableValue<TValue> val)
        {
            return val.ToString();
        }

        /// <summary>
        /// Returns a formatted version of this value
        /// <para/>
        /// (NAME:VALUE)
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.converter.ToString(this.value);
        }

        /// <summary>
        /// Gets a value indicating whether or not the value of this <see cref="ConfigurableValue{TValue}"/> has been saved to disk
        /// </summary>
        public bool IsStored
        {
            get
            {
                bool exists = Config.Configuration.AppSettings.Settings.AllKeys.Contains(this.name);
                if (!exists)
                    return false;
                else
                    return Config.Configuration.AppSettings.Settings[this.name].Value == this.converter.ToString(this.value);
            }
        }
    }

    /// <summary>
    /// Wrapper for <see cref="ConfigurableValue{T}"/> with T as string and the converter set to a new instance of <see cref="StringConverter"/>
    /// </summary>
    public class ConfigurableStringValue : ConfigurableValue<string>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ConfigurableStringValue"/>
        /// </summary>
        /// <param name="name">The name of the key for the value</param>
        /// <param name="defaultValue">The default value (if no value was found in the configuration file)</param>
        public ConfigurableStringValue(string name, string defaultValue)
            : base(name, new StringConverter(), defaultValue) { }
    }

    /// <summary>
    /// An interface that can be used to implement a simple string to {TValue} converter
    /// </summary>
    /// <typeparam name="TValue">The type of value we're dealing with</typeparam>
    public interface IValueConverter<TValue>
    {
        /// <summary>
        /// Converts the specified string value to a <typeparamref name="TValue"/> type
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        TValue Convert(string value);

        /// <summary>
        /// Converts the specified <typeparamref name="TValue"/> type to a string
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        string ToString(TValue value);
    }


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
