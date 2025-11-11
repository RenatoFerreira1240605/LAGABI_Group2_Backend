using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NeuroNexusBackend.DTOs;
using NeuroNexusBackend.Services;

namespace NeuroNexusBackend.Controllers
{

    /// <summary>Deck management endpoints (requires X-User header).</summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DecksController : ControllerBase
    {
        private readonly IDeckService _svc;
        public DecksController(IDeckService svc) => _svc = svc;

        private long RequireUser()
        {
            if (!Request.Headers.TryGetValue("X-User", out var v) || !long.TryParse(v, out var id))
                throw new BadHttpRequestException("Missing or invalid X-User header", 401);
            return id;
        }

        [HttpPost]
        public async Task<ActionResult<object>> Create([FromBody] DeckCreateRequestDTO body, CancellationToken ct)
        {
            var userId = RequireUser();
            var id = await _svc.CreateAsync(userId, body, ct);
            return Ok(new { deck_id = id });
        }

        [HttpGet]
        public async Task<ActionResult<List<DeckResponseDTO>>> List(CancellationToken ct)
        {
            var userId = RequireUser();
            return Ok(await _svc.ListAsync(userId, ct));
        }
        [HttpPost("{deckId:long}/cards")]
        public async Task<ActionResult<object>> AddCard([FromRoute] long deckId, [FromBody] DeckCardItemDTO body, CancellationToken ct)
        {
            var userId = RequireUser();
            var ok = await _svc.AddCardAsync(userId, deckId, body.CardId, body.Qty <= 0 ? (short)1 : body.Qty, ct);
            return ok ? Ok(new { ok = true }) : NotFound();
        }

        [HttpDelete("{deckId:long}/cards/{cardId:long}")]
        public async Task<ActionResult<object>> RemoveCard([FromRoute] long deckId, [FromRoute] long cardId, [FromQuery] short qty = 1, CancellationToken ct = default)
        {
            var userId = RequireUser();
            var ok = await _svc.RemoveCardAsync(userId, deckId, cardId, qty, ct);
            return ok ? Ok(new { ok = true }) : NotFound();
        }

    }
}
