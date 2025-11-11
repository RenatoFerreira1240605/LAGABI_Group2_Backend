using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NeuroNexusBackend.DTOs;
using NeuroNexusBackend.Services;

namespace NeuroNexusBackend.Controllers
{


    /// <summary>Geospatial spawn endpoints (create, nearby, claim, catch).</summary>
    [ApiController]
    [Route("api/[controller]")]
    public class SpawnsController : ControllerBase
    {
        private readonly ISpawnService _svc;
        public SpawnsController(ISpawnService svc) => _svc = svc;

        [HttpPost]
        public async Task<ActionResult<object>> Create([FromBody] SpawnCreateRequestDTO body, CancellationToken ct)
        {
            var id = await _svc.CreateAsync(body, ct);
            return Ok(new { spawn_id = id });
        }

        [HttpGet("nearby")]
        public async Task<ActionResult<NearbyResponseDTO>> Nearby([FromQuery] double lat, [FromQuery] double lon, [FromQuery] int radiusM = 200, CancellationToken ct = default)
            => Ok(await _svc.NearbyAsync(lat, lon, radiusM, ct));

        [HttpPost("{id:long}/claim")]
        public async Task<ActionResult<object>> Claim([FromRoute] long id, CancellationToken ct)
            => await _svc.ClaimAsync(id, ct) ? Ok(new { ok = true }) : Conflict();

        [HttpPost("{id:long}/catch")]
        public async Task<ActionResult<object>> Catch([FromRoute] long id, CancellationToken ct)
            => await _svc.CatchAsync(id, ct) ? Ok(new { ok = true }) : NotFound();
    }
}
