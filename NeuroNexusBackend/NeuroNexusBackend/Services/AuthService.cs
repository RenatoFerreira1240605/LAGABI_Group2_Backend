using NeuroNexusBackend.Repos;
using static NeuroNexusBackend.Services.AuthService;

namespace NeuroNexusBackend.Services
{
    /// <summary>
    /// Auth service: creates guest users with unique handles.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IUserRepo _users;
        public AuthService(IUserRepo users) => _users = users;

        public async Task<GuestResponse> CreateGuestAsync(GuestRequest req, CancellationToken ct)
        {
            // Build a friendly handle if none provided.
            var handle = string.IsNullOrWhiteSpace(req.Handle)
                ? $"guest_{Guid.NewGuid().ToString("N")[..6]}"
                : req.Handle.Trim();

            var u = await _users.CreateGuestAsync(handle, ct);
            return new GuestResponse(u.Id, u.Handle);
        }
    }
}

