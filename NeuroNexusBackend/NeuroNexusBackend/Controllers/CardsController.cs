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
        [HttpGet("collection/{userId}")]
        public async Task<ActionResult<List<UserCardDTO>>> GetUserCollection([FromRoute]long userId)
        {
            var cards = await _svc.GetUserCollectionAsync(userId);
            return Ok(cards);
        }
        /// <summary>
        /// Obtém uma carta específica do workshop de um user para edição.
        /// GET /api/cards/workshop/123?userId=6
        /// </summary>
        [HttpGet("workshop/{userId}")]
        public async Task<ActionResult<List<WorkshopCardUpsertDTO>>> GetWorkshopCard(
            [FromQuery] long? cardId,
            [FromQuery] string? status,
            [FromRoute] long userId)
        {
            var cards = await _svc.GetWorkshopCardsAsync(userId, cardId, status);
            if (cards is null)
                return NotFound();

            return Ok(cards);
        }
        [HttpPost("workshop/{userId}")]
        public async Task<ActionResult<WorkshopCardUpsertDTO>> UpsertWorkshopCard([FromRoute] long userId, [FromBody] WorkshopCardUpsertDTO dto)
        {
            var result = await _svc.UpsertWorkshopCardAsync(userId, dto);
            return Ok(result);
        }

            [HttpDelete("{cardId}")]
        public async Task<ActionResult<string>> DeleteCard([FromRoute] long cardId)
        {
            try
            {
                await _svc.DeleteCard(cardId);
            }
            catch (Exception e)
            {
                return NotFound(e.Message);
            }
            return Ok("Card Removed");
        }

        /// <summary>
        /// Apaga uma carta.
        /// DELETE /api/cards/?userId=6
        /// body = WorkshopCardUpsertDTO
        /// </summary>
        [HttpPost("inventory/{userId}")]
        public async Task<ActionResult> GrantInventory([FromRoute] long userId, [FromBody] List<AddToInventoryDTO> grants)
        {
            if (grants == null || grants.Count == 0)
                return BadRequest("Empty Payload.");

            await _svc.GrantCardsToUserAsync(userId, grants);

            return Ok(new { ok = true, count = grants.Count });
        }


    }
}
