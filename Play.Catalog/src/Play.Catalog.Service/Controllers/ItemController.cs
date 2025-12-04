using Microsoft.AspNetCore.Mvc;

namespace Play.Catalog.Service.Controllers;

using Dtos;
using Entities;
using Repositories;

[ApiController]
[Route("items")]
public class ItemController(IRepository<Item> itemsRepository) : ControllerBase
{
    private readonly IRepository<Item> _itemsRepository = itemsRepository ?? throw new ArgumentNullException(nameof(itemsRepository));

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ItemDto>>> GetAllAsync()
    {
        var items = (await _itemsRepository.GetAllItemsAsync()).Select(item => item.AsDto());
        return Ok(items);

    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ItemDto>> GetItemByIdAsync(Guid id)
    {
        var item = await _itemsRepository.GetItemByIdAsync(id);

        if (item is null)
        {
            return NotFound();
        }

        return Ok(item.AsDto());
    }

    [HttpPost]
    public async Task<ActionResult<ItemDto>> CreateItemAsync([FromBody] CreateItemDto? createItem)
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

        await _itemsRepository.CreateItemAsync(item);

        // MVC routing strips "Async" from action names, so supply the action name without the suffix
        return CreatedAtAction(nameof(GetItemByIdAsync).Replace("Async", ""), new { id = item.Id }, item);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> UpdateItem(Guid id, [FromBody] UpdateItemDto? updateItemDto)
    {
        if (updateItemDto is null)
            return BadRequest();

        var existingItem = await _itemsRepository.GetItemByIdAsync(id);

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
            await _itemsRepository.CreateItemAsync(created);
            return CreatedAtAction(nameof(GetItemByIdAsync).Replace("Async", ""), new { id = created.Id }, created.AsDto());
        }

        existingItem.Name = updateItemDto.Name;
        existingItem.Description = updateItemDto.Description;
        existingItem.Price = updateItemDto.Price;

        await _itemsRepository.UpdateItemAsync(existingItem);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteItemAsync(Guid id)
    {
        var itemToDelete = await _itemsRepository.GetItemByIdAsync(id);

        if (itemToDelete is null)
        {
            return NotFound();
        }

        await _itemsRepository.RemoveItemAsync(id);
        return NoContent();
    }

}