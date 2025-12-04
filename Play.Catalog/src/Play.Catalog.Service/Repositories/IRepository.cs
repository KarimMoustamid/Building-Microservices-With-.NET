namespace Play.Catalog.Service.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Entities;

    public interface IRepository<T> where T : IEntity
    {
        Task<IReadOnlyCollection<T>> GetAllItemsAsync();
        Task<T?> GetItemByIdAsync(Guid id);
        Task<T> CreateItemAsync(T entity);
        Task UpdateItemAsync(T entity);
        Task RemoveItemAsync(Guid id);
    }
}