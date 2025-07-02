using KunFarm.BLL.DTOs.Request;
using KunFarm.BLL.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KunFarm.Presentation.Controllers
{
    [Route("/regular-shop")]
    [ApiController]
    public class RegularShopController : ControllerBase
    {
        private readonly IRegularShopSlotService _regularShopService;

        public RegularShopController(IRegularShopSlotService regularShopService)
        {
            _regularShopService = regularShopService;
        }

        [HttpGet("{playerId:int}")]            
        public async Task<IActionResult> GetAll([FromRoute] int playerId)
        {
            var result = await _regularShopService.GetShopItem(playerId);
            if (result.Data is { } && result.Data.Count > 0)
                return Ok(result);       
            return NotFound(result);
        }

        [HttpPost("buy/{playerId:int}")]
        public async Task<IActionResult> BuyItems([FromRoute] int playerId, [FromBody] BuyItemRequestList request)
        {
            var result = await _regularShopService.BuyItem(playerId, request.Items);
            if (result.Data is { } && result.Data.Count > 0)
                return Ok(result);
            return NotFound(result);
        }
    }
}
