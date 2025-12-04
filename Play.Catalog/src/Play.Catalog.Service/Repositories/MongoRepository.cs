namespace Play.Catalog.Service.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Entities;
    using MongoDB.Driver;

    public class MongoRepository<T> : IRepository<T> where T : IEntity
    {
        // NOTE : Docker Command : docker run -d --rm --name mongoCatalog -p 27017:27017 -v mongodbdata:/data/db mongo

        // MongoDB collection handle typed to our entity class.
        private readonly IMongoCollection<T> _dbCollection;
        private readonly FilterDefinitionBuilder<T> _filterBuilder;

        public MongoRepository(IMongoDatabase database, string collectionName)
        {
            _dbCollection = database.GetCollection<T>(collectionName);
            _filterBuilder = Builders<T>.Filter;
        }

        #region Repository Methods

        public async Task<IReadOnlyCollection<T>> GetAllItemsAsync() => await _dbCollection.Find(_filterBuilder.Empty).ToListAsync();

        public async Task<T?> GetItemByIdAsync(Guid id)
        {
            FilterDefinition<T> filter = _filterBuilder.Eq(entity => entity.Id, id);
            return await _dbCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<T> CreateItemAsync(T entity)
        {
            if (entity is null) throw new ArgumentNullException(nameof(entity));

            T? exist = await GetItemByIdAsync(entity.Id);
            if (exist != null) throw new ArgumentException("Entity already exists");

            await _dbCollection.InsertOneAsync(entity);
            return entity;
        }

        public async Task UpdateItemAsync(T entity)
        {
            if (entity is null) throw new ArgumentNullException(nameof(entity));

            FilterDefinition<T> filter = _filterBuilder.Eq(existingEntity => existingEntity.Id, entity.Id);

            await _dbCollection.ReplaceOneAsync(filter, entity, new ReplaceOptions { IsUpsert = false });
        }

        public async Task RemoveItemAsync(Guid id)
        {
            FilterDefinition<T> filter = _filterBuilder.Eq(existingEntity => existingEntity.Id, id);
            await _dbCollection.DeleteOneAsync(filter);
        }

        #endregion
    }
}