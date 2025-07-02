using KunFarm.BLL.DTOs.Request;
using KunFarm.BLL.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KunFarm.Presentation.Controllers
{
    [Route("inventory")]
    [ApiController]
    public class InventoryController : ControllerBase
    {

        private readonly IInventorySlotService _inventoryService;

        public InventoryController(IInventorySlotService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        [HttpGet("{playerId:int}")]
        public async Task<IActionResult> GetAll([FromRoute] int playerId)
        {
            var result = await _inventoryService.GetInventorySlots(playerId);
            if (result.Data is { } && result.Data.Count > 0)
                return Ok(result);
            return NotFound(result);
        }

        [HttpPost("save/{playerId:int}")]
        public async Task<IActionResult> SaveInventory([FromRoute] int playerId, [FromBody] InventorySaveList request)
        {
            var result = await _inventoryService.SaveInventory(playerId, request);
            if (result.Data is { } && result.Data.Count > 0)
                return Ok(result);
            return NotFound(result);
        }

        [HttpPut("update-slot/{playerId:int}")]
        public async Task<IActionResult> UpdateInventorySlot([FromRoute] int playerId, [FromBody] UpdateInventorySlotRequest request)
        {
            var result = await _inventoryService.UpdateInventorySlot(playerId, request);
            if (result.Data is { })
                return Ok(result);
            return BadRequest(result);
        }

        [HttpPut("batch-update/{playerId:int}")]
        public async Task<IActionResult> BatchUpdateInventory([FromRoute] int playerId, [FromBody] BatchUpdateInventoryRequest request)
        {
            var result = await _inventoryService.BatchUpdateInventory(playerId, request);
            if (result.Data is { } && result.Data.Count > 0)
                return Ok(result);
            return BadRequest(result);
        }

    }
}
