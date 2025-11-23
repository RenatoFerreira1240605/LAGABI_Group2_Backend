using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NeuroNexusBackend.DTOs;
using NeuroNexusBackend.Services;

namespace NeuroNexusBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExpansionsController : ControllerBase
    {
        private readonly ICardService _cardService;

        public ExpansionsController(ICardService cardService)
        {
            _cardService = cardService;
        }
        [HttpPost("{code}/purchase")]
        public async Task<IActionResult> Purchase(string code, [FromQuery] long userId)
        {
            try
            {
                await _cardService.PurchaseExpansionAsync(userId, code);
                return Ok(new { ok = true });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { ok = false, error = ex.Message });
            }
        }

        [HttpGet("available")]
        public async Task<ActionResult<List<ExpansionDTO>>> GetAvailable([FromQuery] long? userId)
        {
            var list = await _cardService.GetExpansionsAsync(userId);
            return Ok(list);
        }
    }
}
