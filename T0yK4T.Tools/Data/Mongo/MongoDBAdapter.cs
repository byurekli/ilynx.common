using System;
using MongoDB.Bson;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace T0yK4T.Tools.Data.Mongo
{
    /// <summary>
    /// A data adapter for a mongo collection
    /// <para/>
    /// Please note that objects stored in the collection have to be "compliant" with the mongodb driver,
    /// <para/>
    /// this means they should have ONE property named "_objectid" of type <see cref="MongoDB.Bson.ObjectId"/>
    /// <para/>
    /// (Or have the attribute <see cref="MongoDB.Bson.Serialization.Attributes.BsonIdAttribute"/> on a property of the same type)
    /// <para/>
    /// Please note that you may need to use <see cref="T0yK4T.Tools.Data.DataSerializer{T}"/> in order to search for a specific object
    /// </summary>
    /// <typeparam name="T">The type of object to store</typeparam>
    public class MongoDBAdapter<T> : IDataAdapter<T>
    {
        private MongoCollection<T> collection;
        private Type type = typeof(T);

        /// <summary>
        /// Initializes a new instance of <see cref="MongoDBAdapter{T}"/>
        /// </summary>
        /// <param name="database"></param>
        public MongoDBAdapter(MongoDatabase database)
        {
            this.collection = database.GetCollection<T>(this.CollectionName);
        }

        /// <summary>
        /// Initializes a new instance of <see cref="MongoDBAdapter{T}"/> and attempts to connect to MongoDB on the specified host+port combination
        /// </summary>
        /// <param name="host">The host to connect to</param>
        /// <param name="port">The port where MongoDB is running on the host</param>
        /// <param name="databasename">The name of the database to use</param>
        public MongoDBAdapter(string host, int port, string databasename)
        {
            MongoServer server = new MongoServer(new MongoServerSettings{ ConnectTimeout = TimeSpan.FromSeconds(5), Server = new MongoServerAddress(host, port)});
            server.Connect();
            MongoDatabase db = server.GetDatabase(new MongoDatabaseSettings(databasename, null, MongoDB.Bson.GuidRepresentation.Standard, new SafeMode(false), true));
            this.collection = db.GetCollection<T>(this.CollectionName);
        }

        /// <summary>
        /// Stores the specified value in the database
        /// <para/>
        /// Same as Update
        /// </summary>
        /// <param name="value"></param>
        public void Store(T value)
        {
            this.collection.Save(value);
        }

        /// <summary>
        /// Gets the name of the underlying collection
        /// </summary>
        public string CollectionName
        {
            get { return this.type.FullName; }
        }

        /// <summary>
        /// Simply returns <see cref="MongoCollection{T}.FindOne()"/>
        /// </summary>
        /// <returns></returns>
        public T GetFirst()
        {
            return this.collection.FindOne();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        public T FindOne(params DataProperty<T>[] properties)
        {
            return this.collection.FindOne(this.BuildQuery(properties));
        }

        /// <summary>
        /// Executes an "update / save" command on the current collection
        /// <para/>
        /// Same as Store
        /// </summary>
        /// <param name="value"></param>
        public void Update(T value)
        {
            this.collection.Save<T>(value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public T FindOne(DataProperty<T> property)
        {
            return this.collection.FindOne(this.BuildQuery(property));
        }

        private QueryComplete BuildQuery(DataProperty<T> property)
        {
            return Query.EQ(property.PropertyName, MongoDB.Bson.BsonValue.Create(property.Value));
        }

        private QueryComplete BuildQuery(IEnumerable<DataProperty<T>> properties)
        {
            List<QueryComplete> subQueries = new List<QueryComplete>();

            foreach (DataProperty<T> property in properties)
                subQueries.Add(this.BuildQuery(property)); //Query.EQ(property.PropertyName, MongoDB.Bson.BsonValue.Create(property.Value)));

            return Query.And(subQueries.ToArray());
        }

        /// <summary>
        /// Attempts to find a list of <typeparamref name="T"/>s in the underlying collection, using the specified <see cref="DataProperty{T}"/> as a filter
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public IEnumerable<T> Find(DataProperty<T> property)
        {
            try
            {
                return this.collection.Find(this.BuildQuery(property));
            }
            catch { return null; }
        }

        /// <summary>
        /// Attempts to find a collection of <typeparamref name="T"/>s in the underlying collection, using the <paramref name="regex"/> paramter as a regular expression and matching on the specified key (<paramref name="key"/>)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="regex"></param>
        /// <returns></returns>
        public IEnumerable<T> Find(string key, Regex regex)
        {
            try
            {
                return this.collection.Find(Query.Matches(key, BsonRegularExpression.Create(regex)));
            }
            catch { return null; }
        }

        /// <summary>
        /// Attempts to find a list of <typeparamref name="T"/>s in the underlying collection using the specified list of <see cref="DataProperty{T}"/> as "filter"
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        public IEnumerable<T> Find(IEnumerable<DataProperty<T>> properties)
        {
            try
            {
                return this.collection.Find(this.BuildQuery(properties));
            }
            catch { return null; }
        }
    }
}
