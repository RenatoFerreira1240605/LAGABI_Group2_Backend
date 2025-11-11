using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NeuroNexusBackend.DTOs;
using NeuroNexusBackend.Services;

namespace NeuroNexusBackend.Controllers
{

    /// <summary>Authentication endpoints (guest creation).</summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;
        public AuthController(IAuthService auth) => _auth = auth;

        /// <summary>Create a guest user for development/testing.</summary>
        [HttpPost("guest")]
        public async Task<ActionResult<GuestResponseDTO>> CreateGuest([FromBody] GuestRequestDTO req, CancellationToken ct)
            => Ok(await _auth.CreateGuestAsync(req, ct));
    }
}
