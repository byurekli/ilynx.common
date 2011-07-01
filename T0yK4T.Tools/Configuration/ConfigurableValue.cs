using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace T0yK4T.Tools.Configuration
{
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
                string sVal = ConfigurationManager.AppSettings[this.name];
                this.value = converter.Convert(sVal);
            }
            catch { this.value = defaultValue; }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ConfigurableValue{TValue}"/> and sets the name and value components to the specified values
        /// </summary>
        /// <param name="name">The name of the value (Key)</param>
        /// <param name="value">The actual value</param>
        /// <param name="converter">The converter to use when saving / retrieving the value from file</param>
        public ConfigurableValue(string name, TValue value, IValueConverter<TValue> converter)
        {
            this.name = name;
            this.value = value;
        }

        /// <summary>
        /// Attempts to store this value in the configuration file
        /// </summary>
        public void Store()
        {
            if (!ConfigurationManager.AppSettings.AllKeys.Contains(this.name))
                ConfigurationManager.AppSettings.Add(this.name, this.converter.ToString(this.value));
            else
                ConfigurationManager.AppSettings[this.name] = this.converter.ToString(this.value);
        }

        /// <summary>
        /// Attempts to load this value from file (returns true if succesful, otherwise false)
        /// </summary>
        /// <returns></returns>
        public bool TryLoad()
        {
            try
            {
                string sVal = ConfigurationManager.AppSettings[this.name];
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
            return string.Format("{0}:{1}", this.name, this.converter.ToString(this.value));
        }
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
}
