using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NeuroNexusBackend.DTOs;
using NeuroNexusBackend.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace NeuroNexusBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MeController : ControllerBase
    {
        private readonly IUserService _userService;

        public MeController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [Authorize] // valida o JWT e popula User.Claims
        public async Task<ActionResult<UserResponseDTO>> GetMe()
        {
            // o TokenService coloca o Id em JwtRegisteredClaimNames.Sub ("sub")

            var userIdClaim =
                 User.FindFirst(ClaimTypes.NameIdentifier) ??
                 User.FindFirst(JwtRegisteredClaimNames.Sub) ??
                 User.FindFirst("sub") ??
                 User.FindFirst("id");

            if (userIdClaim == null)
                return Unauthorized();

            if (!long.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized();

            var user = await _userService.GetUserById(userId);
            if (user == null)
                return NotFound();

            var dto = new UserResponseDTO
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
