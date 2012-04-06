using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Net;
using System.ComponentModel;

namespace T0yK4T.Configuration
{
    /// <summary>
    /// A simple configurable value (Uses the builtin ConfigurationManager (<see cref="System.Configuration.ConfigurationManager"/>))
    /// </summary>
    public abstract class ConfigurableValue
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ConfigurableValue"/> and sets it's key property to the specified value
        /// </summary>
        /// <param name="key"></param>
        public ConfigurableValue(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");
            this.Key = key;
            try { Config.LoadedValues.Add(this.Key, this); }
            catch (ArgumentException) { throw; }
        }

        /// <summary>
        /// Attempts to store this value in the configuration file
        /// </summary>
        public abstract void Store();

        /// <summary>
        /// Attempts to load this value from file (returns true if succesful, otherwise false)
        /// </summary>
        /// <returns></returns>
        public abstract bool TryLoad();

        /// <summary>
        /// Gets the value of this <see cref="ConfigurableValue"/>
        /// </summary>
        public abstract object Value { get; set; }

        /// <summary>
        /// Gets the key of this <see cref="ConfigurableValue"/>
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// Gets a value indicating whether or not the value of this <see cref="ConfigurableValue"/> has been saved to disk
        /// </summary>
        public abstract bool IsStored { get; }

        public abstract event EventHandler ValueChanged;
    }

    /// <summary>
    /// A simple configurable value (Uses the builtin ConfigurationManager (<see cref="System.Configuration.ConfigurationManager"/>))
    /// </summary>
    /// <typeparam name="T">The type of value to store</typeparam>
    public class ConfigurableValue<T> : ConfigurableValue
    {
        private T value;
        private IValueConverter<T> converter;

        /// <summary>
        /// Initializes a new instance of <see cref="ConfigurableValue{T}"/> and attempts to load it's value from the
        /// <para/>
        /// configuration manager (using the specified <see cref="IValueConverter{TValue}"/> to convert the retrieved string to a <typeparamref name="T"/> type
        /// </summary>
        /// <param name="key">The key of the value to look for</param>
        /// <param name="converter">The <see cref="IValueConverter{TValue}"/> to use for string to <typeparamref name="T"/> conversion</param>
        /// <param name="defaultValue">The default value to use if the value could not be retrieved from the configuration file</param>
        public ConfigurableValue(string key, IValueConverter<T> converter, T defaultValue)
            : base(key)
        {
            this.converter = converter;
            try
            {
                if (Config.Configuration.AppSettings.Settings.AllKeys.Contains(key))
                {
                    string sVal = Config.Configuration.AppSettings.Settings[this.Key].Value;
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
        public override void Store()
        {
            
            if (!Config.Configuration.AppSettings.Settings.AllKeys.Contains(this.Key))
                Config.Configuration.AppSettings.Settings.Add(this.Key, this.converter.ToString(this.value));
            else
                Config.Configuration.AppSettings.Settings[this.Key].Value = this.converter.ToString(this.value);
            try { Config.Configuration.Save(ConfigurationSaveMode.Modified); }
            catch { }
        }

        /// <summary>
        /// Attempts to load this value from file (returns true if succesful, otherwise false)
        /// </summary>
        /// <returns></returns>
        public override bool TryLoad()
        {
            try
            {
                string sVal = Config.Configuration.AppSettings.Settings[this.Key].Value;
                this.value = this.converter.Convert(sVal);
            }
            catch { return false; }
            return true;
        }

        /// <summary>
        /// Gets or Sets the value of this <see cref="ConfigurableValue{T}"/>
        /// </summary>
        public T TValue
        {
            get { return this.value; }
            set
            {
                this.value = value;
                this.OnValueChanged();
            }
        }

        /// <summary>
        /// Simply retrieves the value of the specified <see cref="ConfigurableValue{T}"/>
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static implicit operator T(ConfigurableValue<T> val)
        {
            return val.value;
        }

        /// <summary>
        /// Simply returns <see cref="ConfigurableValue{T}.ToString()"/>
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static explicit operator string(ConfigurableValue<T> val)
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
        /// Gets a value indicating whether or not the value of this <see cref="ConfigurableValue{T}"/> has been saved to disk
        /// </summary>
        public override bool IsStored
        {
            get
            {
                bool exists = Config.Configuration.AppSettings.Settings.AllKeys.Contains(this.Key);
                if (!exists)
                    return false;
                else
                    return Config.Configuration.AppSettings.Settings[this.Key].Value == this.converter.ToString(this.value);
            }
        }

        /// <summary>
        /// Gets the value of this <see cref="ConfigurableValue{T}"/> as an object
        /// </summary>
        public override object Value
        {
            get { return this.value; }
            set
            {
                if (!(value is T))
                    throw new InvalidOperationException("The specified value is not valid for this type of ConfigurableValue");
                this.value = (T)value;
                this.OnValueChanged();
            }
        }

        private void OnValueChanged()
        {
            if (this.ValueChanged != null)
                this.ValueChanged(this, new EventArgs());
        }

        public override event EventHandler ValueChanged;
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
}
