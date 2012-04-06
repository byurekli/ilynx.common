using System.Configuration;
using System.Collections.Generic;

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
        
        /// <summary>
        /// Gets a reference to a dictionary of all the currently loaded <see cref="ConfigurableValue"/>s
        /// </summary>
        public static Dictionary<string, ConfigurableValue> LoadedValues
        {
            get { return loadedValues; }
        }
    }
}