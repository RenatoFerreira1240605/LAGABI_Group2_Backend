using NeuroNexusBackend.DTOs;
using NeuroNexusBackend.Repos;
using static NeuroNexusBackend.Services.AuthService;

namespace NeuroNexusBackend.Services
{
    /// <summary>Auth service: creates guest users with unique handles.</summary>
    using Microsoft.EntityFrameworkCore;
    using NeuroNexusBackend.Data;
    using NeuroNexusBackend.DTOs;
    using NeuroNexusBackend.Models;

    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        public AuthService(AppDbContext db) => _db = db;

        public async Task<GuestResponseDTO> CreateGuestAsync(GuestRequestDTO req, CancellationToken ct)
        {
            var baseHandle = string.IsNullOrWhiteSpace(req.Handle) ? "user" : req.Handle!.Trim().ToLowerInvariant();
            const int maxRetries = 20;

            for (int i = 0; i < maxRetries; i++)
            {
                var handle = i == 0 ? baseHandle : $"{baseHandle}{i + 1}";
                if (await _db.Users.AnyAsync(u => u.Handle == handle, ct)) continue;

                var user = new User
                {
                    Handle = handle,
                    DisplayName = string.IsNullOrWhiteSpace(req.DisplayName) ? handle : req.DisplayName!.Trim(),
                    CreatedAt = DateTime.UtcNow
                };

                _db.Users.Add(user);
                try
                {
                    await _db.SaveChangesAsync(ct);
                    return new GuestResponseDTO { UserId = user.Id, Handle = user.Handle };
                }
                catch (DbUpdateException)
                {
                    // colisão concorrente — tenta próximo sufixo
                }
            }

            throw new InvalidOperationException("Could not generate a unique handle.");
        }
    }
}



