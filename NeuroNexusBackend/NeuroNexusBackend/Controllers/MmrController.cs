using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NeuroNexusBackend.Data;
using NeuroNexusBackend.Models;
using NeuroNexusBackend.Services;

namespace NeuroNexusBackend.Controllers
{
    /// <summary>Hidden MMR endpoints (simple Elo updates).</summary>
    [ApiController]
    [Route("api/[controller]")]
    public class MmrController : ControllerBase
    {
        private readonly IMmrService _mmr;
        private readonly AppDbContext _db;

        public MmrController(IMmrService mmr, AppDbContext db)
        {
            _mmr = mmr;
            _db = db;
        }

        /// <summary>Get current hidden rating for a user in a mode.</summary>
        [HttpGet("{userId:long}")]
        public async Task<ActionResult<object>> Get([FromRoute] long userId, [FromQuery] string mode = "pvp1v1", CancellationToken ct = default)
            => Ok(new { rating = await _mmr.GetAsync(userId, mode, ct) });

        /// <summary>Resolve a match and update both players' ratings.</summary>
        [HttpPost("match/resolve")]
        public async Task<ActionResult<object>> Resolve([FromBody] ResolveReq req, CancellationToken ct)
        {
            var match = new Match
            {
                Mode = req.Mode,
                Player1Id = req.P1,
                Player2Id = req.P2,
                Status = "finished",
                CreatedAt = DateTime.UtcNow,
                StartedAt = DateTime.UtcNow.AddMinutes(-5),
                EndedAt = DateTime.UtcNow,
                Winner = req.Winner,
                P1Points = req.P1Points,
                P2Points = req.P2Points,
                P1RatingStart = await _mmr.GetAsync(req.P1, req.Mode, ct),
                P2RatingStart = await _mmr.GetAsync(req.P2, req.Mode, ct)
            };
            _db.Matches.Add(match);
            await _db.SaveChangesAsync(ct);

            await _mmr.UpdateAfterMatchAsync(req.P1, req.P2, req.Mode, req.Winner, ct);
            return Ok(new { ok = true, match_id = match.Id });
        }

        public struct ResolveReq
        {
            public long P1 { get; set; }
            public long P2 { get; set; }
            public string Mode { get; set; }
            public int Winner { get; set; }
            public short P1Points { get; set; }
            public short P2Points { get; set; }
        }
    }
}
