using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QrCodeGenerator.Api
{
    public class MongoRepository : IMongoRepository
    {
        private static IMongoClient _client;
        private readonly MongoConfig _mongoConfig;
        private readonly IMongoDatabase _database;

        public MongoRepository(MongoConfig mongoConfig)
        {
            _mongoConfig = mongoConfig ?? throw new ArgumentNullException(nameof(mongoConfig));

            if (_client == null)
            {
                if (!_mongoConfig.EnableQuickMongoConnectionCycle)
                {
                    MongoDefaults.MaxConnectionIdleTime = TimeSpan.FromMinutes(1);
                }

                _client = new MongoClient(_mongoConfig.ConnectionString);
            }

            _database = _client.GetDatabase(_mongoConfig.Database);

            // This code will tell MongoDB to always serialise the fields it can and ignore any extras. Otherwise it errors during serialisation if someone adds a new field.
            var conventionPack = new ConventionPack { new IgnoreExtraElementsConvention(true) };
            ConventionRegistry.Register("IgnoreExtraElements", conventionPack, type => true);
        }

        /// <summary>
        /// Finds the document against a given id.
        /// </summary>
        /// <param name="id">id of the document.</param>
        /// <param name="collection">The collection name.</param>
        /// <param name="cancellationToken">Token used to cancel an async request.</param>
        /// <typeparam name="T">A generic type inherited from class to deserialise the response to.</typeparam>
        /// <returns>An object deserialized from the searched document.</returns>
        public async Task<T> FindByIdAsync<T>(Guid id, string collection, CancellationToken cancellationToken = default)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", id.ToString());
            var result = (await _database.GetCollection<BsonDocument>(collection).FindAsync(filter)).SingleOrDefault(cancellationToken);

            return result is null
                ? default
                : BsonSerializer.Deserialize<T>(result);
        }

        public async Task<T> FindRandom<T>(string collection, CancellationToken cancellationToken = default)
        {
            var result = _database.GetCollection<BsonDocument>(collection).Aggregate()
               .AppendStage<BsonDocument>("{ $sample: { size: 1 } }");

            var obj = await result.FirstAsync();
            return result is null
                ? default
                : BsonSerializer.Deserialize<T>(obj);
        }

        /// <summary>
        /// Inserts a document, given its serialised form.
        /// </summary>
        /// <param name="collection">The colleciton name.</param>
        /// <param name="serialisedObj">The serialised object to insert.</param>
        /// <param name="id">The ID of the document.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task InsertAsync(string collection, string serialisedObj, Guid guid)
        {
            var col = _database.GetCollection<BsonDocument>(collection);
            var bson = BsonSerializer.Deserialize<BsonDocument>(serialisedObj).Add(new BsonElement("_id", new BsonString(guid.ToString())));

            await col.InsertOneAsync(bson);
        }

        /// <summary>
        /// Inserts a document, given its serialised form.
        /// </summary>
        /// <param name="collection">The colleciton name.</param>
        /// <param name="serialisedObj">The serialised object to insert.</param>
        /// <param name="id">The ID of the document.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task InsertAsync(string collection, string serialisedObj)
        {
            var col = _database.GetCollection<BsonDocument>(collection);
            var bson = BsonSerializer.Deserialize<BsonDocument>(serialisedObj);

            await col.InsertOneAsync(bson);
        }

        /// <summary>
        /// Saves a new document.
        /// </summary>
        /// <param name="collection">The name of the collection.</param>
        /// <param name="value">The object to be saved.</param>
        /// <param name="id">An optional primary id which we want MongoDB to use.</param>
        /// <typeparam name="T">A generic type inherited from class to insert into the document collection.</typeparam>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task InsertAsync<T>(string collection, T value)
            where T : class
        {
            var col = _database.GetCollection<BsonDocument>(collection);
            var bson = value.ToBsonDocument();
            //await col.InsertOneAsync(bson.Add(new BsonElement("_id", new BsonInt32(id))));
            await col.InsertOneAsync(bson);
        }

        public Task InsertAsync<T>(string collection, T value, int id) where T : class
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Replaces a document, given its serialised form.
        /// </summary>
        /// <param name="collection">The collection name.</param>
        /// <param name="serialisedObj">The serialised object to insert.</param>
        /// <param name="id">The ID of the document.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ReplaceAsync(string collection, string serialisedObj, int id)
        {
            var col = _database.GetCollection<BsonDocument>(collection);
            var filter = Builders<BsonDocument>.Filter.Eq("_id", new BsonInt32(id));
            var bson = BsonSerializer.Deserialize<BsonDocument>(serialisedObj).Add(new BsonElement("_id", new BsonInt32(id)));
            await col.ReplaceOneAsync(filter, bson, new ReplaceOptions { IsUpsert = true });
        }

        /// <summary>
        /// Replaces a document, given its serialised form.
        /// </summary>
        /// <param name="collection">The collection name.</param>
        /// <param name="value">The object to insert.</param>
        /// <param name="id">The ID of the document.</param>
        /// <typeparam name="T">A generic type inherited from class to upsert into the document collection.</typeparam>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ReplaceAsync<T>(string collection, T value, int id)
            where T : class
        {
            var col = _database.GetCollection<BsonDocument>(collection);
            var filter = Builders<BsonDocument>.Filter.Eq("_id", new BsonInt32(id));
            var bson = value.ToBsonDocument().Add(new BsonElement("_id", new BsonInt32(id)));
            await col.ReplaceOneAsync(filter, bson, new ReplaceOptions { IsUpsert = true });
        }
    }
}
