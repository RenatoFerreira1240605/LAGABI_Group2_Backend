using Microsoft.AspNetCore.Mvc;
using NeuroNexusBackend.DTOs;
using NeuroNexusBackend.Services;

namespace NeuroNexusBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;
        private readonly IGoogleDeviceAuthService _googleDeviceAuth;

        public AuthController(IAuthService auth, IGoogleDeviceAuthService googleDeviceAuth)
        {
            _auth = auth;
            _googleDeviceAuth = googleDeviceAuth;
        }

        //[HttpPost("guest")]
        //public async Task<ActionResult<GuestResponseDTO>> CreateGuest(
        //    [FromBody] GuestRequestDTO req,
        //    CancellationToken ct)
        //    => Ok(await _auth.CreateGuestAsync(req, ct));

        // Novo: start do device flow
        [HttpPost("google/device/start")]
        public async Task<ActionResult<DeviceStartResponseDTO>> StartGoogleDeviceFlow(CancellationToken ct)
        {
            var result = await _googleDeviceAuth.StartAsync(ct);
            return Ok(result);
        }

        // Novo: poll do device flow
        [HttpGet("google/device/poll")]
        public async Task<ActionResult<DevicePollResponseDTO>> PollGoogleDeviceFlow([FromQuery] Guid requestId)
        {
            var result = await _googleDeviceAuth.PollAsync(requestId);
            return Ok(result);
        }
    }
}
