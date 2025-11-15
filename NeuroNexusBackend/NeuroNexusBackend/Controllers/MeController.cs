using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NeuroNexusBackend.DTOs;
using NeuroNexusBackend.Repos;

namespace NeuroNexusBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MeController : ControllerBase
    {
        private readonly IUserRepo _userRepo;

        public MeController(IUserRepo userRepo)
        {
            _userRepo = userRepo;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<MeResponseDTO>> Get(CancellationToken ct)
        {
            // sub = Id interno do utilizador (ver TokenService)
            var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                      ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(sub))
                return Unauthorized();

            if (!long.TryParse(sub, out var userId))
                return Unauthorized();

            var user = await _userRepo.GetAsync(userId, ct);
            if (user == null)
                return Unauthorized();

            var dto = new MeResponseDTO
            {
                Id = user.Id,
                Handle = user.Handle,
                DisplayName = user.DisplayName,
                Email = user.Email,
                Provider = user.ExternalProvider
            };

            return Ok(dto);
        }
    }
}
