using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using T0yK4T.Tools;

namespace T0yK4T.Tools
{
    /// <summary>
    /// A class used to "manage" Modules internally in toychat
    /// <para/>
    /// Currently only -loads- Modules!
    /// </summary>
    public class AssemblyLoader : ComponentBase
    {
        /// <summary>
        /// Attempts to load any <typeparam name="TInterface"/>s that are defined in the specified assembly
        /// <para/>
        /// This method will return an empty list if no classes deriving from the specified interface were found
        /// <para/>
        /// (Or none of them have an empty constructor)
        /// </summary>
        /// <param name="assembly">The assembly to look in</param>
        /// <returns>A list of {TInterface}s</returns>
        public IEnumerable<TInterface> LoadTypes<TInterface>(Assembly assembly)
        {
            List<TInterface> retVal = new List<TInterface>();
            try
            {
                Type[] types = assembly.GetTypes();
                retVal.AddRange(types.Where(t2 => t2.GetInterfaces()
                    .Any(tInterface => tInterface == typeof(TInterface))).Where(t => !t.IsAbstract && !t.IsInterface)
                    .Select<Type, TInterface>(tFinal =>
                    {
                        base.LogInformation("Attempting to activate {0}", tFinal.FullName);
                        try
                        {
                            return (TInterface)Activator.CreateInstance(tFinal);
                        }
                        catch
                        {
                            base.LogError("Could not activate {0}", tFinal.FullName);
                            return default(TInterface);
                        }
                    })
                    .Where(f => !object.ReferenceEquals(f, default(TInterface))));
            }
            catch (Exception er) { this.LogException(er, MethodBase.GetCurrentMethod()); }
            return retVal;
        }

        /// <summary>
        /// Attempts to load all the assemblies in the specified directory
        /// </summary>
        /// <param name="dirname">The directory to search and "load"</param>
        /// <param name="excludeFiles">Specifies which files to exclude during module search</param>
        /// <returns></returns>
        public IEnumerable<T> LoadDirectory<T>(string dirname, params string[] excludeFiles)
        {
            List<T> instances = new List<T>();
            try
            {
                string[] fNames = Directory.GetFiles(dirname, "*.dll");
                Type TType = typeof(T);
                this.LogInformation("Found {0} files possibly containing Modules of Type {1}", fNames.Length, TType.FullName);
                foreach (string fName in fNames)
                {
                    try
                    {
                        if (excludeFiles != null && excludeFiles.Contains(Path.GetFileName(fName)))
                            base.LogInformation("Skipping excluded assembly {0}", fName);
                        else
                        {
                            this.LogInformation("Attempting to load Modules of type {1} defined in Assembly [{0}]", fName, TType.FullName);
                            instances.AddRange(LoadTypes<T>(Assembly.LoadFrom(fName)));
                        }
                    }
                    catch (Exception er) { this.LogException(er, MethodBase.GetCurrentMethod()); continue; }
                }
            }
            catch (Exception er) { this.LogException(er, MethodBase.GetCurrentMethod()); }
            return instances;
        }
    }
}
