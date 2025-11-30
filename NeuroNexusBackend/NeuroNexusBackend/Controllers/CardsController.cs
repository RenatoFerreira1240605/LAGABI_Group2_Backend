using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.UserSecrets;
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
        public async Task<ActionResult<List<CardRuntimeDTO>>> Runtime([FromQuery] string? suit, [FromQuery] string? rarity, [FromQuery] string? trigger, [FromQuery] string? effect)
        {
            var cards = await _svc.SearchAsync(suit, rarity, trigger, effect);
            return Ok(cards);
        }
        /// <summary>Devolve a coleção de cartas de um utilizador (inventário completo).</summary>
        [HttpGet("collection")]
        public async Task<ActionResult<List<UserCardDTO>>> GetUserCollection(long userId)
        {
            var cards = await _svc.GetUserCollectionAsync(userId);
            return Ok(cards);
        }
        /// <summary>
        /// Obtém uma carta específica do workshop de um user para edição.
        /// GET /api/cards/workshop/123?userId=6
        /// </summary>
        [HttpGet("workshop")]
        public async Task<ActionResult<WorkshopCardUpsertDTO>> GetWorkshopCard(
            [FromQuery] long cardId,
            [FromQuery] long userId)
        {
            var card = await _svc.GetWorkshopCardAsync(userId, cardId);
            if (card is null)
                return NotFound();

            return Ok(card.Value);
        }

        /// <summary>
        /// Cria ou atualiza uma carta do workshop (draft/active).
        /// POST /api/cards/workshop?userId=6
        /// body = WorkshopCardUpsertDTO
        /// </summary>
        [HttpPost("workshop")]
        public async Task<ActionResult<WorkshopCardUpsertDTO>> UpsertWorkshopCard(
            [FromQuery] long userId,
            [FromBody] WorkshopCardUpsertDTO dto)
        {
            var result = await _svc.UpsertWorkshopCardAsync(userId, dto);
            return Ok(result);
        }


    }
}
