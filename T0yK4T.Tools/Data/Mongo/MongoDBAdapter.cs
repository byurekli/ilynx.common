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
        /// Attempts to find every unique value of <paramref name="key"/>
        /// </summary>
        /// <param name="key">The key to look for</param>
        /// <returns></returns>
        public IEnumerable<T2> Distinct<T2>(string key)
        {
            return this.collection.Distinct(key).Cast<T2>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        public T FindOne(params DataProperty<T>[] properties)
        {
            return this.collection.FindOne(this.BuildQuery(properties, BooleanOperator.AND));
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

        private QueryComplete BuildQuery(IEnumerable<DataProperty<T>> properties, BooleanOperator op)
        {
            List<QueryComplete> subQueries = new List<QueryComplete>();

            foreach (DataProperty<T> property in properties)
                subQueries.Add(this.BuildQuery(property)); //Query.EQ(property.PropertyName, MongoDB.Bson.BsonValue.Create(property.Value)));

            switch (op)
            {
                case BooleanOperator.AND:
                    return Query.And(subQueries.ToArray());
                case BooleanOperator.OR:
                    return Query.Or(subQueries.ToArray());
                default:
                    return Query.Or(subQueries.ToArray());
            }
        }

        /// <summary>
        /// Executes <see cref="MongoDB.Driver.MongoCollection{T}.FindAll()"/>.ToArray()
        /// </summary>
        /// <returns></returns>
        public IEnumerable<T> GetAll()
        {
            return this.collection.FindAll();
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
        /// Deletes all records containing the specified property with it's value
        /// </summary>
        /// <param name="property"></param>
        public void Delete(DataProperty<T> property)
        {
            try
            {
                this.collection.Remove(this.BuildQuery(property));
            }
            catch { }
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
        /// Attempts to find instances of <typeparamref name="T"/> in the underlying datastore who's <paramref name="key"/> property match the specified <paramref name="regex"/>
        /// <para/>
        /// (Please note that this may only be possible on string types!
        /// </summary>
        /// <param name="key"></param>
        /// <param name="regex"></param>
        /// <returns></returns>
        public void Delete(string key, Regex regex)
        {
            try
            {
                this.collection.Remove(Query.Matches(key, BsonRegularExpression.Create(regex)));
            }
            catch { }
        }

        /// <summary>
        /// Attempts to find a collection of <typeparamref name="T"/>s in the underlying collection using the specified <paramref name="matchFields"/> to make matches
        /// </summary>
        /// <param name="matchFields"></param>
        /// <param name="op"></param>
        /// <returns></returns>
        public IEnumerable<T> Find(BooleanOperator op, params KeyValuePair<string, Regex>[] matchFields)
        {
            try
            {
                return this.collection.Find(this.BuildQuery(matchFields, op));
            }
            catch { return null; }
        }

        /// <summary>
        /// This method combines the funcionality of <see cref="Delete(string, Regex)"/> with <see cref="Find(BooleanOperator, KeyValuePair{string, Regex}[])"/>
        /// </summary>
        /// <param name="op"></param>
        /// <param name="matchFields"></param>
        public void Delete(BooleanOperator op, params KeyValuePair<string, Regex>[] matchFields)
        {
            try
            {
                this.collection.Remove(this.BuildQuery(matchFields, op));
            }
            catch { }
        }

        private QueryComplete BuildQuery(IEnumerable<KeyValuePair<string, Regex>> fields, BooleanOperator op)
        {
            List<QueryComplete> subQueries = new List<QueryComplete>();
            foreach (KeyValuePair<string, Regex> field in fields)
                subQueries.Add(this.BuildQuery(field.Key, field.Value));
            switch (op)
            {
                case BooleanOperator.AND:
                    return Query.And(subQueries.ToArray());
                case BooleanOperator.OR:
                    return Query.Or(subQueries.ToArray());
                default:
                    return Query.And(subQueries.ToArray());
            }
        }

        private QueryComplete BuildQuery(string key, Regex value)
        {
            if (string.IsNullOrEmpty(key) || value == null)
                throw new ArgumentNullException();
            return Query.Matches(key, new BsonRegularExpression(value));
        }

        /// <summary>
        /// Attempts to find a list of <typeparamref name="T"/>s in the underlying collection using the specified list of <see cref="DataProperty{T}"/> as "filter"
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="op"></param>
        /// <returns></returns>
        public IEnumerable<T> Find(IEnumerable<DataProperty<T>> properties, BooleanOperator op)
        {
            try
            {
                return this.collection.Find(this.BuildQuery(properties, op));
            }
            catch { return null; }
        }

        /// <summary>
        /// Deletes all records in the underlying datasource that match the specified properties, combining them with the specified boolean operator
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="op"></param>
        public void Delete(IEnumerable<DataProperty<T>> properties, BooleanOperator op)
        {
            try
            {
                this.collection.Remove(this.BuildQuery(properties, op));
            }
            catch { }
        }

        /// <summary>
        /// Attempts to find distinct values of <typeparamref name="T2"/> matching the field <paramref name="key"/> in the underlying collection
        /// </summary>
        /// <typeparam name="T2">The type of object to look for</typeparam>
        /// <param name="key">The key / field name to retrieve</param>
        /// <param name="op">The BooleanOperator to use for multiple "matchFields"</param>
        /// <param name="matchFields">The fields to match</param>
        /// <returns></returns>
        public IEnumerable<T2> Distinct<T2>(string key, BooleanOperator op, params KeyValuePair<string, Regex>[] matchFields)
        {
            if (matchFields == null)
                return this.collection.Distinct(key).Cast<T2>();
            else
            {
                QueryComplete q = this.BuildQuery(matchFields, op);
                return this.collection.Distinct(key, q).Cast<T2>();
            }
        }

        /// <summary>
        /// Attempts to count the values in the underlying database, optionally using the specified filter
        /// </summary>
        /// <param name="op"></param>
        /// <param name="matchFields"></param>
        /// <returns></returns>
        public int Count(BooleanOperator op, params KeyValuePair<string, Regex>[] matchFields)
        {
            if (matchFields == null)
                return this.collection.Count();
            else
                return this.collection.Count(this.BuildQuery(matchFields, op));
        }
    }
}
