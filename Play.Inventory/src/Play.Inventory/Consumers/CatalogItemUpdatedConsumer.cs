namespace Play.Inventory.Consumers
{
    using Catalog.Contracts;
    using Common.Repositories;
    using Entities;
    using MassTransit;

    public class CatalogItemUpdatedConsumer : IConsumer<Contracts.CatalogItemUpdated>
    {
        private readonly IRepository<CatalogItem> repository;

        public CatalogItemUpdatedConsumer(IRepository<CatalogItem> repository)
        {
            this.repository = repository;
        }

        public async Task Consume(ConsumeContext<Contracts.CatalogItemUpdated> context)
        {
            var message = context.Message;

            var item = await repository.GetByIdAsync(message.ItemId);

            if (item == null)
            {
                item = new CatalogItem
                {
                    Id = message.ItemId,
                    Name = message.Name,
                    Description = message.Description
                };

                await repository.CreateAsync(item);
            }
            else
            {
                item.Name = message.Name;
                item.Description = message.Description;

                await repository.UpdateAsync(item);
            }
        }
    }
}