using KunFarm.BLL.DTOs.Request;
using KunFarm.BLL.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KunFarm.Presentation.Controllers
{
    [Route("/online-shop")]
    [ApiController]
    public class OnlineShopController : ControllerBase
    {

        private readonly IOnlineShopService _onlineShopService;

        public OnlineShopController(IOnlineShopService onlineShopService)
        {
            _onlineShopService = onlineShopService;
        }

        [HttpGet("{playerId:int}")]
        public async Task<IActionResult> GetAll([FromRoute] int playerId)
        {
            var result = await _onlineShopService.GetRandomSoldItem(playerId);
            if (result.Data is { } && result.Data.Count > 0)
                return Ok(result);
            return NotFound(result);
        }

        [HttpPost("sell/{playerId:int}")]
        public async Task<IActionResult> SellItems([FromRoute] int playerId, [FromBody] SellItemRequest request)
        {
            var result = await _onlineShopService.SellItem(playerId, request);
            if (result.Code == 200)
                return Ok(result);
            return NotFound(result);
        }


        [HttpPost("buy/{playerId:int}")]
        public async Task<IActionResult> BuyItems([FromRoute] int playerId, [FromBody] List<int> request)
        {
            var result = await _onlineShopService.BuyItem(playerId, request);
            if (result.Code == 200)
                return Ok(result);
            return NotFound(result);
        }

    }
}
