using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.IO;
using System.Reflection;

namespace Hasherer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            IEnumerable<string> loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().Select<Assembly, string>(a => a.Location);
            foreach (string referencedButNotLoaded in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll").Where(s => !loadedAssemblies.Contains(s, StringComparer.InvariantCultureIgnoreCase)))
                AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(referencedButNotLoaded));
        }
    }
}
