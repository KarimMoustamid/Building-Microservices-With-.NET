namespace Play.Catalog.Service.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Entities;
    using MongoDB.Driver;

    public class ItemsRepository(IMongoDatabase database) : IItemsRepository
    {
        // NOTE : Docker Command : docker run -d --rm --name mongoCatalog -p 27017:27017 -v mongodbdata:/data/db mongo

        // Name of the MongoDB collection where Item documents are stored.
        private const string collectionName = "items";
        // MongoDB collection handle typed to our entity class.
        private readonly IMongoCollection<Item> dbCollection = database.GetCollection<Item>(collectionName);
        private readonly FilterDefinitionBuilder<Item> filterBuilder = Builders<Item>.Filter;

        // This does not create network traffic by itself; operations on `_items` will talk to the server.

        #region Repository Methods

        public async Task<IReadOnlyCollection<Item>> GetAllItemsAsync() => await dbCollection.Find(filterBuilder.Empty).ToListAsync();

        public async Task<Item?> GetItemByIdAsync(Guid id)
        {
            FilterDefinition<Item> filter = filterBuilder.Eq(entity => entity.Id, id);
            return await dbCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<Item> CreateItemAsync(Item entity)
        {
            if (entity is null) throw new ArgumentNullException(nameof(entity));

           Item? exist = await GetItemByIdAsync(entity.Id);
           if(exist != null) throw new ArgumentException("Item already exists");

            await dbCollection.InsertOneAsync(entity);
            return entity;
        }

        public async Task UpdateItemAsync(Item entity)
        {
            if (entity is null) throw new ArgumentNullException(nameof(entity));

            FilterDefinition<Item> filter = filterBuilder.Eq(existingEntity => existingEntity.Id, entity.Id);

            await dbCollection.ReplaceOneAsync(filter, entity, new ReplaceOptions { IsUpsert = false });
        }

        public async Task RemoveItemAsync(Guid id)
        {
            FilterDefinition<Item> filter = filterBuilder.Eq(existingEntity => existingEntity.Id, id);
            await dbCollection.DeleteOneAsync(filter);
        }

        #endregion
    }
}