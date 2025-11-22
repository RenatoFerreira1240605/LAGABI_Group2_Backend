using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using NeuroNexusBackend.DTOs;
using NeuroNexusBackend.Repos;
using NeuroNexusBackend.Services;

namespace NeuroNexusBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _service;

        public UserController(IUserService service)
        {
            _service = service;
        }

        [HttpGet("GetById")]
        public async Task<ActionResult<UserResponseDTO>> GetById([Required] long userId)
        {
            var user = await _service.GetUserById(userId);
            if (user == null)
                return NoContent();

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
        [HttpGet("GetByEmail")]
        public async Task<ActionResult<UserResponseDTO>> GetByEmail([Required]string email)
        {            
            var user = await _service.GetUserByEmail(email);
            if (user == null)
                return NoContent();

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
