namespace Play.Catalog.Service.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Entities;

    public interface IItemsRepository
    {
        Task<IReadOnlyCollection<Item>> GetAllItemsAsync();
        Task<Item?> GetItemByIdAsync(Guid id);
        Task<Item> CreateItemAsync(Item entity);
        Task UpdateItemAsync(Item entity);
        Task RemoveItemAsync(Guid id);
    }
}