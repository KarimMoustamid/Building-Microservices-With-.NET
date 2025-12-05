namespace Play.Inventory
{
    using Dto;
    using Entities;

    public static class Extentions
    {
        public static Dtos.InventoryItemDto AsDto(this InventoryItem item)
        {
            return new Dtos.InventoryItemDto(item.CatalogItemId, item.Quantity, item.AcquiredDate);
        }
    }
}