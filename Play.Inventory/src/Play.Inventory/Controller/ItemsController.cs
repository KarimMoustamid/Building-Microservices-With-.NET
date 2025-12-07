using Microsoft.AspNetCore.Mvc;
using Play.Inventory.Dto;
using Play.Inventory.Entities;

namespace Play.Inventory.Controller
{
    using Client;
    using Play.Common;
    using Play.Common.Repositories;

    [ApiController]
    [Route("items")]
    public class ItemsController(IRepository<InventoryItem> itemsRepository, CatalogClient catalogClient) : ControllerBase
    {
        [HttpGet("{userId:guid}")]
        public async Task<ActionResult<IEnumerable<Dtos.InventoryItemDto>>> GetAsync(Guid userId)
        {
            if (userId == Guid.Empty)
                return BadRequest("userId is required");

            var catalogItems = await catalogClient.GetCatalogItemsAsync();
            var inventoryItemEntities = await itemsRepository.GetAllAsync(item => item.UserId == userId);

            var inventoryItemDtos = inventoryItemEntities.Select(inventoryItem =>
            {
                var catalogItem = catalogItems.Single(catalogItem => catalogItem.Id == inventoryItem.CatalogItemId);
                return inventoryItem.AsDto(catalogItem.Name, catalogItem.Description);
            });

            return Ok(inventoryItemDtos);        }

        [HttpPost]
        public async Task<ActionResult> PostAsync([FromBody] Dtos.GrantItemDto grantItem)
        {
            if (grantItem == null || grantItem.UserId == Guid.Empty || grantItem.CatalogItemId == Guid.Empty || grantItem.Quantity <= 0)
                return BadRequest("Invalid payload");

            InventoryItem existingItem = await itemsRepository.GetByIdAsync(item => item.UserId == grantItem.UserId && item.CatalogItemId == grantItem.CatalogItemId);

            if (existingItem == null)
            {
                var item = new InventoryItem
                {
                    //Id = Guid.NewGuid(),
                    UserId = grantItem.UserId,
                    CatalogItemId = grantItem.CatalogItemId,
                    Quantity = grantItem.Quantity,
                    AcquiredDate = DateTimeOffset.UtcNow
                };
                await itemsRepository.CreateAsync(item);
                var catalogItem = (await catalogClient.GetCatalogItemsAsync())
                    .SingleOrDefault(c => c.Id == item.CatalogItemId);

                // Return location header pointing to the GET endpoint for the user
                return CreatedAtAction(nameof(GetAsync).Replace("Async", ""), new { userId = item.UserId }, item.AsDto(catalogItem.Name, catalogItem.Description));

            }
            else
            {
               existingItem.Quantity += grantItem.Quantity;
               await itemsRepository.UpdateAsync(existingItem);
               var catalogItem = (await catalogClient.GetCatalogItemsAsync())
                   .SingleOrDefault(c => c.Id == existingItem.CatalogItemId);

               return Ok(existingItem.AsDto(catalogItem.Name, catalogItem.Description));
            }

        }
    }
}