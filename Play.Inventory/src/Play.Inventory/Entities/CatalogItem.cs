namespace Play.Inventory.Entities
{
    using Common.Repositories;

    public class CatalogItem : IEntity
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }
}