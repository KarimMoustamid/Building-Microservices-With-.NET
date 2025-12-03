using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
namespace Play.Catalog.Service.Dtos;

    [ApiController]
    [Route("items")]
    public class ItemController : ControllerBase
    {
        // NOTE : This is temporary. We will replace it with a proper database later.
        private static readonly List<ItemDto> items = new List<ItemDto>()
        {
            new ItemDto(
                Guid.NewGuid(),
                "Health Potion",
                "Restores 50 HP instantly.",
                4.99M,
                DateTimeOffset.UtcNow.AddDays(-10)
            ),
            new ItemDto(
                Guid.NewGuid(),
                "Iron Sword",
                "Standard melee weapon with moderate damage.",
                29.99M,
                DateTimeOffset.UtcNow.AddDays(-8)
            ),
            new ItemDto(
                Guid.NewGuid(),
                "Wooden Shield",
                "Basic shield that reduces incoming damage.",
                14.50M,
                DateTimeOffset.UtcNow.AddDays(-6)
            ),
            new ItemDto(
                Guid.NewGuid(),
                "Longbow",
                "Ranged weapon with high accuracy.",
                39.00M,
                DateTimeOffset.UtcNow.AddDays(-4)
            ),
            new ItemDto(
                Guid.NewGuid(),
                "Leather Helmet",
                "Light head protection, increases defense slightly.",
                12.75M,
                DateTimeOffset.UtcNow.AddDays(-2)
            )
        };

        [HttpGet]
        public ActionResult<IEnumerable<ItemDto>> GetAll() => Ok(items);

        [HttpGet("{id:guid}")]
        public ActionResult<ItemDto> GetItemById(Guid id)
        {
            var item = items.Find(i => i.Id == id);
            if (item is null)
            {
                return NotFound();
            }

            return Ok(item);
        }

        [HttpPost]
        public ActionResult<ItemDto> CreateItem([FromBody] CreateItemDto item)
        {
            if (item is null)
                return BadRequest();

            var newItem = new ItemDto(Guid.NewGuid(), item.Name, item.Description, item.Price, DateTimeOffset.UtcNow);

            items.Add(newItem);

            return CreatedAtAction(nameof(GetItemById), new { id = newItem.Id }, newItem);
        }

        [HttpPut("{id:guid}")]
        public ActionResult UpdateItem(Guid id, [FromBody] UpdateItemDto item)
        {
            if (item is null)
                return BadRequest();

            var existingItem = items.Find(i => i.Id == id);

            if (existingItem is null)
            {
                return NotFound();
            }

            var updatedItem = existingItem with
            {
                Name = item.Name,
                Description = item.Description,
                Price = item.Price
            };

            var index = items.IndexOf(existingItem);
            items[index] = updatedItem;

            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public ActionResult DeleteItem(Guid id)
        {
            var itemToDelete = items.Find(i => i.Id == id);
            if (itemToDelete is null)
            {
                return NotFound();
            }

            items.RemoveAll(i => i.Id == id);
            return NoContent();
        }

    }