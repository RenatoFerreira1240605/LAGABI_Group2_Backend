using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NeuroNexusBackend.DTOs;
using NeuroNexusBackend.Services;

namespace NeuroNexusBackend.Controllers
{


    /// <summary>
    /// Geospatial spawn endpoints (create, nearby, claim, catch).
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SpawnsController : ControllerBase
    {
        private readonly ISpawnService _svc;
        public SpawnsController(ISpawnService svc) => _svc = svc;

        /// <summary>Create a spawn at a given location (admin/dev only).</summary>
        [HttpPost]
        public async Task<ActionResult<object>> Create([FromBody] SpawnCreateRequestDTO body, CancellationToken ct)
        {
            var id = await _svc.CreateAsync(body, ct);
            return Ok(new { spawn_id = id });
        }

        /// <summary>List active spawns near (lat,lon) within radius meters.</summary>
        [HttpGet("nearby")]
        public async Task<ActionResult<NearbyResponseDTO>> Nearby([FromQuery] double lat, [FromQuery] double lon, [FromQuery] int radiusM = 200, CancellationToken ct = default)
            => Ok(await _svc.NearbyAsync(lat, lon, radiusM, ct));

        /// <summary>Claim a spawn (optimistic lock: active → claimed).</summary>
        [HttpPost("{id:guid}/claim")]
        public async Task<ActionResult<object>> Claim([FromRoute] Guid id, CancellationToken ct)
            => await _svc.ClaimAsync(id, ct) ? Ok(new { ok = true }) : Conflict();

        /// <summary>Catch a spawn (active/claimed → caught).</summary>
        [HttpPost("{id:guid}/catch")]
        public async Task<ActionResult<object>> Catch([FromRoute] Guid id, CancellationToken ct)
            => await _svc.CatchAsync(id, ct) ? Ok(new { ok = true }) : NotFound();
    }
}
