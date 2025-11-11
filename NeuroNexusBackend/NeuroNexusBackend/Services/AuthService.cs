using NeuroNexusBackend.DTOs;
using NeuroNexusBackend.Repos;
using static NeuroNexusBackend.Services.AuthService;

namespace NeuroNexusBackend.Services
{
    /// <summary>Auth service: creates guest users with unique handles.</summary>
    public class AuthService : IAuthService
    {
        private readonly IUserRepo _users;
        public AuthService(IUserRepo users) => _users = users;

        public async Task<GuestResponseDTO> CreateGuestAsync(GuestRequestDTO req, CancellationToken ct)
        {
            var handle = string.IsNullOrWhiteSpace(req.Handle)
                ? $"guest_{Guid.NewGuid():N}".Substring(0, 12)
                : req.Handle.Trim();

            var u = await _users.CreateGuestAsync(handle, ct);
            // Se o teu GuestResponseDTO não tiver construtor, usa inicializador:
            // return new GuestResponseDTO { UserId = u.Id, Handle = u.Handle };
            return new GuestResponseDTO(u.Id, u.Handle);
        }
    }
}

