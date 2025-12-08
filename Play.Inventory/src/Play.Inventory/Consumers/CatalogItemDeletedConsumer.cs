namespace Play.Inventory.Consumers
{
    using Catalog.Contracts;
    using Common.Repositories;
    using Entities;
    using MassTransit;

    public class CatalogItemDeletedConsumer : IConsumer<Contracts.CatalogItemDeleted>
    {
        private readonly IRepository<CatalogItem> repository;

        public CatalogItemDeletedConsumer(IRepository<CatalogItem> repository)
        {
            this.repository = repository;
        }

        public async Task Consume(ConsumeContext<Contracts.CatalogItemDeleted> context)
        {
            var message = context.Message;

            var item = await repository.GetByIdAsync(message.ItemId);

            if (item == null)
            {
                return;
            }

            await repository.RemoveAsync(message.ItemId);
        }

    }
}