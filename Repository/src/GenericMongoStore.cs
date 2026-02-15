using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace Repository
{
    public class EntityNotFoundException(string? message) : Exception(message) { }

    public class GenericMongoStore<T> : IGenericStore<T>
        where T : BaseEntity
    {
        protected readonly IMongoCollection<T> Collection;

        public GenericMongoStore(string dbHost, string dbName, string dbCollection)
        {
            string connectionString = "mongodb://" + dbHost + "/?retryWrites=true&w=majority";
            var client = new MongoClient(connectionString);
            Collection = client.GetDatabase(dbName).GetCollection<T>(dbCollection);
        }

        public async Task<long> Count()
        {
            return await Collection.CountDocumentsAsync(Builders<T>.Filter.Empty);
        }

        public async Task<T> Insert(T entity)
        {
            await Collection.InsertOneAsync(entity);

            return entity;
        }

        public async Task<T> Update(T entity)
        {
            var filter =
                Builders<T>.Filter.Eq("_id", entity.Id)
                & Builders<T>.Filter.Eq("Version", entity.Version);

            entity.Version += 1;
            var resp = await Collection.ReplaceOneAsync(filter, entity);
            if (resp.ModifiedCount != 1)
            {
                string message =
                    "Entity with Id: "
                    + entity.Id
                    + " and Version: "
                    + entity.Version
                    + " not found";
                throw new EntityNotFoundException(message);
            }

            return entity;
        }

        public async Task<T?> FindBy<TV>(string key, TV value)
        {
            var filter = Builders<T>.Filter.Eq(key, value);

            return (await Collection.FindAsync(filter)).FirstOrDefault();
        }

        public async Task<T?> FindBy(Expression<Func<T, bool>> expr)
        {
            await Task.CompletedTask;
            return Collection.AsQueryable().Where(expr).ToList().FirstOrDefault();
        }

        public async Task Clear()
        {
            await Collection.DeleteManyAsync(Builders<T>.Filter.Empty);
        }

        public async Task<List<T>> ReadAll()
        {
            return (await Collection.FindAsync(Builders<T>.Filter.Empty)).ToList();
        }

        public async Task<T?> Find(string id)
        {
            return await FindBy(e => e.Id == id);
        }

        public async Task<bool> Delete(string id)
        {
            var filter = Builders<T>.Filter.Eq(e => e.Id, id);
            var res = await Collection.DeleteOneAsync(filter);

            return res.DeletedCount == 1;
        }
    }
}
