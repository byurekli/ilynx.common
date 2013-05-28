using System.Configuration;
using System.Collections.Generic;
using System;

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

        private static Dictionary<string, ConfigurableValue> loadedValues = new Dictionary<string, ConfigurableValue>();
        
        ///// <summary>
        ///// Gets a reference to a dictionary of all the currently loaded <see cref="ConfigurableValue"/>s
        ///// </summary>
        //public static Dictionary<string, ConfigurableValue> LoadedValues
        //{
        //    get { return loadedValues; }
        //}

        /// <summary>
        /// Attempts to load a <see cref="ConfigurableValue{T}"/> with the specified <paramref name="key"/>
        /// </summary>
        /// <typeparam name="T">The type of <see cref="ConfigurableValue{T}"/> to load</typeparam>
        /// <param name="key">The key of the value</param>
        /// <param name="converter">The converter to use (<see cref="IValueConverter{T}"/>)</param>
        /// <param name="defaultValue">The default value to assign to the value of the <see cref="ConfigurableValue{T}"/> if it is not found in file</param>
        /// <returns></returns>
        /// <exception cref="InvalidCastException">This exception is thrown if a value with the same key has already been loaded, but is not of the specified type <typeparamref name="T"/></exception>
        public static ConfigurableValue<T> GetValue<T>(string key, IValueConverter<T> converter, T defaultValue)
        {
            ConfigurableValue untyped;
            ConfigurableValue<T> ret;
            if (loadedValues.TryGetValue(key, out untyped))
            {
                try { ret = (ConfigurableValue<T>)untyped; }
                catch (InvalidCastException e) { throw new InvalidCastException(string.Format("The specified Configurable value exists, but is not of type {0}", typeof(T).FullName), e); }
                catch { throw; }
            }
            else
            {
                ret = new ConfigurableValue<T>(key, converter, defaultValue);
                loadedValues.Add(key, ret);
            }
            return ret;
        }
    }
}