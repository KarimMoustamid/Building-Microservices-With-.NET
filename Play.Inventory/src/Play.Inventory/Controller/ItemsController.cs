using Microsoft.AspNetCore.Mvc;
using Play.Inventory.Dto;
using Play.Inventory.Entities;

namespace Play.Inventory.Controller
{
    using Common;
    using Common.Repositories;

    [ApiController]
    [Route("[controller]")]
    public class ItemsController(IRepository<InventoryItem> itemRepository) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Dtos.InventoryItemDto>>> GetAsync([FromQuery] Guid userId)
        {
            if (userId == Guid.Empty)
                return BadRequest("userId is required");

            var allItems = (await itemRepository.GetAllAsync(item => item.UserId == userId)).Select(i => i.AsDto()).ToList();

            return Ok(allItems);
        }

        [HttpPost]
        public async Task<ActionResult> PostAsync([FromBody] Dtos.GrantItemDto grantItem)
        {
            if (grantItem == null || grantItem.UserId == Guid.Empty || grantItem.CatalogItemId == Guid.Empty || grantItem.Quantity <= 0)
                return BadRequest("Invalid payload");

            InventoryItem existingItem = await itemRepository.GetByIdAsync(item => item.UserId == grantItem.UserId && item.CatalogItemId == grantItem.CatalogItemId);

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
                await itemRepository.CreateAsync(item);
                // Return location header pointing to the GET endpoint for the user
                return CreatedAtAction(nameof(GetAsync), new { userId = item.UserId }, item.AsDto());

            }
            else
            {
               existingItem.Quantity += grantItem.Quantity;
               await itemRepository.UpdateAsync(existingItem);
               return Ok(existingItem.AsDto());
            }

        }
    }
}