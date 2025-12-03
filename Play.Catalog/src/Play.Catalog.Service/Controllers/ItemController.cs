using Microsoft.AspNetCore.Mvc;
namespace Play.Catalog.Service.Dtos;
using Entities;
using Repositories;

[ApiController]
    [Route("items")]
    public class ItemController : ControllerBase
    {
        private readonly ItemsRepository itemsRepository = new ItemsRepository();

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemDto>>> GetAllAsync()
        {
            var items = (await itemsRepository.GetAllItemsAsync()).Select(item => item.AsDto());
            return Ok(items);

        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ItemDto>> GetItemByIdAsync(Guid id)
        {
            var item = await itemsRepository.GetItemByIdAsync(id);

            if (item is null)
            {
                return NotFound();
            }

            return Ok(item.AsDto());
        }

        [HttpPost]
        public async Task<ActionResult<ItemDto>> CreateItemAsync([FromBody] CreateItemDto createItem)
        {
            if (createItem is null)
                return BadRequest();

            var item = new Item()
            {
                Name = createItem.Name,
                Description = createItem.Description,
                Price = createItem.Price,
                CreatedDate = DateTimeOffset.UtcNow
            };

            await itemsRepository.CreateItemAsync(item);

            return CreatedAtAction(nameof(GetItemByIdAsync), new { id = item.Id }, item);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult> UpdateItem(Guid id, [FromBody] UpdateItemDto updateItemDto)
        {
            if (updateItemDto is null)
                return BadRequest();

            var existingItem = await itemsRepository.GetItemByIdAsync(id);

            if (existingItem is null)
            {
                var created = new Item
                {
                    Id = id,
                    Name = updateItemDto.Name,
                    Description = updateItemDto.Description,
                    Price = updateItemDto.Price,
                    CreatedDate = DateTimeOffset.UtcNow
                };
                await itemsRepository.CreateItemAsync(created);
                return CreatedAtAction(nameof(GetItemByIdAsync), new { id = created.Id }, created.AsDto());
            }

            existingItem.Name = updateItemDto.Name;
            existingItem.Description = updateItemDto.Description;
            existingItem.Price = updateItemDto.Price;

            await itemsRepository.UpdateItemAsync(existingItem);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> DeleteItemAsync(Guid id)
        {
            var itemToDelete = await itemsRepository.GetItemByIdAsync(id);

            if (itemToDelete is null)
            {
                return NotFound();
            }

            await itemsRepository.RemoveItemAsync(id);
            return NoContent();
        }

    }