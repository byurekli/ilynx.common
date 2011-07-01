using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace T0yK4T.Tools.Data
{
    /// <summary>
    /// A simple implementation of <see cref="IDataSerializer{T}"/>
    /// <para/>
    /// Please note that this serializer will only serialize Types that have properties marked with <see cref="DataPropertyAttribute"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DataSerializer<T> : IDataSerializer<T> where T : new()
    {
        private Dictionary<string, PropertyInfo> serializedFields = new Dictionary<string, PropertyInfo>();
        private Dictionary<string, DataProperty<T>> template = new Dictionary<string, DataProperty<T>>();
        private Type handledType = typeof(T);

        /// <summary>
        /// Initializes a new instance of <see cref="DataSerializer{T}"/> and builds the internal format table
        /// </summary>
        public DataSerializer()
        {
            this.BuildFormat();
        }

        /// <summary>
        /// ...
        /// </summary>
        private void BuildFormat()
        {
            PropertyInfo[] properties = handledType.GetProperties();
            foreach (PropertyInfo pInfo in properties)
            {
                if (Attribute.IsDefined(pInfo, typeof(DataPropertyAttribute)) && pInfo.CanRead && pInfo.CanWrite)
                {
                    this.serializedFields.Add(pInfo.Name, pInfo);
                    this.template.Add(pInfo.Name, new DataProperty<T>(pInfo.Name, null, pInfo.PropertyType));
                }
            }
        }

        /// <summary>
        /// Serializes the specified value using the internal format table
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public IEnumerable<DataProperty<T>> Serialize(T value)
        {
            List<DataProperty<T>> retVal = new List<DataProperty<T>>();
            foreach (KeyValuePair<string, PropertyInfo> kvp in this.serializedFields)
            {
                string name = kvp.Key;
                PropertyInfo pInfo = kvp.Value;
                DataProperty<T> finalProperty = new DataProperty<T>(this.template[name]);
                finalProperty.Value = pInfo.GetValue(value, null);
                retVal.Add(finalProperty);
                //retVal.Add(new DataProperty<T> { Value = kvp.Value.GetValue(value, null), PropertyName = kvp.Key, DataType =  });
            }
            return retVal;
        }

        /// <summary>
        /// Deserializes the speicifed collection of properties in to a new instance of {T} with it's properties set to the specified values
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        public T Deserialize(IEnumerable<DataProperty<T>> fields)
        {
            T instance = new T();
            foreach (DataProperty<T> df in fields)
                this.serializedFields[df.PropertyName].SetValue(instance, df.Value, null);

            return instance;
        }

        /// <summary>
        /// Attempts to build a collection of set values from the specified generic instance - <paramref name="val"/>
        /// </summary>
        /// <param name="val">The value to get a filter for</param>
        /// <returns></returns>
        public IEnumerable<DataProperty<T>> GetUsableFilter(T val)
        {
            List<DataProperty<T>> filter = new List<DataProperty<T>>();
            foreach (KeyValuePair<string, PropertyInfo> kvp in this.serializedFields)
            {
                object propertyValue = kvp.Value.GetValue(val, null);
                if (propertyValue != GetDefaultValue(kvp.Value.PropertyType))
                    filter.Add(new DataProperty<T>(kvp.Key, propertyValue, kvp.Value.PropertyType));
            }
            return filter;
        }

        /// <summary>
        /// Gets the template used when serializing and deserializing data using this <see cref="DataSerializer{T}"/>
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, DataProperty<T>> GetDataTemplate()
        {
            return new Dictionary<string, DataProperty<T>>(this.template);
        }

        /// <summary>
        /// Gets the default value for the specified type <paramref name="t"/>
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static object GetDefaultValue(Type t)
        {
            if (t.IsValueType)
                return Activator.CreateInstance(t);
            return null;
        }
    }

    /// <summary>
    /// Attribute that should be used on properties that should be "serialized" by the <see cref="DataSerializer{T}"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited=false)]
    public class DataPropertyAttribute : Attribute
    {
        /// <summary>
        /// Empty constructor
        /// </summary>
        public DataPropertyAttribute()
        {
            
        }
    }

    /// <summary>
    /// A structure used to represent a single property in <see cref="DataSerializer{T}"/>
    /// </summary>
    /// <typeparam name="T">Used to constrain types</typeparam>
    public struct DataProperty<T>
    {
        private object value;
        private string name;
        private Type dataType;

        /// <summary>
        /// Gets or Sets the value of this property
        /// </summary>
        public object Value
        {
            get { return this.value; }
            set { this.value = value; }
        }

        /// <summary>
        /// Gets or Sets the name of this property
        /// </summary>
        public string PropertyName
        {
            get { return this.name; }
            set { this.name = value; }
        }

        /// <summary>
        /// Gets or Sets the type of data that this <see cref="DataProperty{T}"/> will eventually contain
        /// </summary>
        public Type DataType
        {
            get { return this.dataType; }
            set { this.dataType = value; }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="DataProperty{T}"/> and sets the <see cref="DataProperty{T}.Value"/> and <see cref="DataProperty{T}.PropertyName"/> to the specified values
        /// </summary>
        /// <param name="name">The name of the property</param>
        /// <param name="value">The value of the property</param>
        /// <param name="dataType">The type of data</param>
        public DataProperty(string name, object value, Type dataType)
        {
            this.value = value;
            this.name = name;
            this.dataType = dataType;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="DataProperty{T}"/> and sets the name and datatype to those contained in the specified template
        /// <para/>
        /// Value is set to null
        /// </summary>
        /// <param name="template"></param>
        public DataProperty(DataProperty<T> template)
        {
            this.value = null;
            this.name = template.name;
            this.dataType = template.dataType;
        }
    }
}
