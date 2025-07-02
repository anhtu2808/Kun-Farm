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

    }
}
