using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace T0yK4T.Tools.Data
{
    /// <summary>
    /// An interface that can be used to implement a simple data store adapter
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDataAdapter<T>
    {
        /// <summary>
        /// Stores the generic type <typeparamref name="T"/> in the datastore
        /// </summary>
        /// <param name="value">The value to store</param>
        void Store(T value);

        /// <summary>
        /// Gets the first instance of <typeparamref name="T"/> available in the datastore
        /// </summary>
        /// <returns></returns>
        T GetFirst();

        /// <summary>
        /// Attempts to find a single element in the underlying datastore with the specified <paramref name="property"/> set
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        T FindOne(DataProperty<T> property);

        /// <summary>
        /// Attempts to find a single element in the underlying datastore with the specified <paramref name="properties"/> set
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        T FindOne(params DataProperty<T>[] properties);

        /// <summary>
        /// Attempts to update the specified value in the underlying datastore
        /// </summary>
        /// <param name="value"></param>
        void Update(T value);

        /// <summary>
        /// Attempts to find instances of <typeparamref name="T"/> in the underlying datastore that have the specified <see cref="DataProperty{T}"/>
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        T[] Find(DataProperty<T> property);

        /// <summary>
        /// Attempts to find instances of <typeparamref name="T"/> in the underlying datastore using the specified <see cref="DataProperty{T}"/> as a filter
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        T[] Find(IEnumerable<DataProperty<T>> properties);

        /// <summary>
        /// Attempts to find instances of <typeparamref name="T"/> in the underlying datastore who's <paramref name="KeyName"/> property match the specified <paramref name="searchPattern"/>
        /// <para/>
        /// (Please note that this may only be possible on string types!
        /// </summary>
        /// <param name="KeyName"></param>
        /// <param name="searchPattern"></param>
        /// <returns></returns>
        T[] Find(string KeyName, Regex searchPattern);

        /// <summary>
        /// Attempts to find a collection of <typeparamref name="T"/>s in the underlying collection using the specified <paramref name="matchFields"/> to make matches
        /// </summary>
        /// <param name="matchFields"></param>
        /// <returns></returns>
        T[] Find(IDictionary<string, Regex> matchFields);
    }
}
