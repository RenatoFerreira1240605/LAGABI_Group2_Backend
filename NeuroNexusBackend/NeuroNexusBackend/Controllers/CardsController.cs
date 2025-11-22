using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NeuroNexusBackend.DTOs;
using NeuroNexusBackend.Services;

namespace NeuroNexusBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CardsController : ControllerBase
    {
        private readonly ICardService _svc;
        public CardsController(ICardService svc) => _svc = svc;

        /// <summary>Bulk upsert de cartas (admin/dev seed).</summary>
        [HttpPost("bulk")]
        public async Task<ActionResult<object>> BulkUpsert([FromBody] List<CardUpsertDTO> body)
        {
            await _svc.UpsertManyAsync(body);
            return Ok(new { ok = true, count = body.Count });
        }

        /// <summary>Pesquisa runtime-friendly (filtra por suit/rarity/trigger/effect).</summary>
        [HttpGet("runtime")]
        public async Task<ActionResult<List<CardRuntimeDTO>>> Runtime(
            [FromQuery] string? suit, [FromQuery] string? rarity,
            [FromQuery] string? trigger, [FromQuery] string? effect)
            => Ok(await _svc.SearchAsync(suit, rarity, trigger, effect));


    }
}
