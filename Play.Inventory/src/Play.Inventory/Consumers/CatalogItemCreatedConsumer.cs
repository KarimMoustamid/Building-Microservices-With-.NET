namespace Play.Inventory.Consumers
{
    using Catalog.Contracts;
    using Common.Repositories;
    using Entities;
    using MassTransit;

    public class CatalogItemCreatedConsumer(IRepository<CatalogItem> repository) : IConsumer<Contracts.CatalogItemCreated>
    {
        public async Task Consume(ConsumeContext<Contracts.CatalogItemCreated> context)
        {
            var message = context.Message;

            var item = await repository.GetByIdAsync(message.ItemId);

            if (item != null)
            {
                return;
            }

            item = new CatalogItem
            {
                Id = message.ItemId,
                Name = message.Name,
                Description = message.Description
            };

            await repository.CreateAsync(item);
        }
    }
}