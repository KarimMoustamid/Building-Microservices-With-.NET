namespace Play.Inventory
{
    using Dto;
    using Entities;

    public static class Extentions
    {
        public static Dtos.InventoryItemDto AsDto(this InventoryItem item, string name, string description)
        {
            return new Dtos.InventoryItemDto(item.CatalogItemId, name, description, item.Quantity, item.AcquiredDate);
        }
    }
}