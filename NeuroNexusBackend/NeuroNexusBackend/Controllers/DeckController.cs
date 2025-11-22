using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NeuroNexusBackend.DTOs;
using NeuroNexusBackend.Services;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace NeuroNexusBackend.Controllers
{

    /// <summary>Deck management endpoints (requires X-User header).</summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DecksController : ControllerBase
    {
        private readonly IDeckService _svc;
        public DecksController(IDeckService svc) => _svc = svc;
               

        [HttpPost]
        public async Task<ActionResult<object>> Create([Required]long userId, [FromBody] DeckCreateRequestDTO body, CancellationToken ct)
        {
            
            var id = await _svc.CreateAsync(userId, body, ct);
            return Ok(new { deck_id = id });
        }
        [HttpGet("UserDeckList")]
        public async Task<ActionResult<List<DeckListDTO>>> GetUserDeckList(
           [Required]long userId,long? deckId,
            CancellationToken ct)
        {
            var decks = await _svc.GetUserDecksAsync(userId, deckId, ct);
            return Ok(decks);
        }

        [HttpGet]
        public async Task<ActionResult<List<DeckResponseDTO>>> List([Required] long userId, CancellationToken ct)
        {
            return Ok(await _svc.ListAsync(userId, ct));
        }

        [HttpPost("{deckId:long}/cards")]
        public async Task<ActionResult<object>> AddCard([Required] long userId,[FromRoute] long deckId, [FromBody] DeckCardDTO body, CancellationToken ct)
        {
            var ok = await _svc.AddCardAsync(userId, deckId, body.CardId, body.Qty <= 0 ? (short)1 : body.Qty, ct);
            return ok ? Ok(new { ok = true }) : NotFound();
        }

        [HttpDelete("{deckId:long}/cards/{cardId:long}")]
        public async Task<ActionResult<object>> RemoveCard([Required] long userId,[FromRoute] long deckId, [FromRoute] long cardId, [FromQuery] short qty = 1, CancellationToken ct = default)
        {
            var ok = await _svc.RemoveCardAsync(userId, deckId, cardId, qty, ct);
            return ok ? Ok(new { ok = true }) : NotFound();
        }

    }
}
