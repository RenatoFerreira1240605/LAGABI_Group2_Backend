using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.EntityFrameworkCore;
using NeuroNexusBackend.Data;
using NeuroNexusBackend.DTOs;
using NeuroNexusBackend.Models;

namespace NeuroNexusBackend.Services
{
    /// <summary>Auth service: creates guest users and external users (e.g. Google).</summary>
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly ICardService _cardService;

        public AuthService(AppDbContext db, ICardService cardService)
        {
            _db = db;
            _cardService = cardService;
            _cardService = cardService;

        }

        public async Task<GuestResponseDTO> CreateGuestAsync(GuestRequestDTO req)
        {
            var baseHandle = string.IsNullOrWhiteSpace(req.Handle)
                ? "user"
                : req.Handle!.Trim().ToLowerInvariant();

            var handle = await GenerateUniqueHandleAsync(baseHandle);

            var user = new User
            {
                Handle = handle,
                DisplayName = string.IsNullOrWhiteSpace(req.DisplayName)
                    ? handle
                    : req.DisplayName!.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return new GuestResponseDTO { UserId = user.Id, Handle = user.Handle };
        }

        /// <summary>
        /// Get or create user for an external IdP (e.g. Google).
        /// </summary>
        public async Task<User> GetOrCreateExternalUserAsync(
            string provider,
            string subject,
            string? email,
            string? displayName)
        {
            // Ver se já existe utilizador para este provider+subject
            var user = await _db.Users
                .FirstOrDefaultAsync(u =>
                    u.ExternalProvider == provider &&
                    u.ExternalSubject == subject);

            if (user != null)
            {
                // Atualiza dados básicos, se quiseres
                var changed = false;

                if (!string.IsNullOrWhiteSpace(email) && user.Email != email)
                {
                    user.Email = email;
                    changed = true;
                }

                if (!string.IsNullOrWhiteSpace(displayName) && user.DisplayName != displayName)
                {
                    user.DisplayName = displayName;
                    changed = true;
                }

                if (changed)
                    await _db.SaveChangesAsync();

                return user;
            }

            // Não existe → criar novo utilizador
            var baseHandle = $"user_{provider}";
            var handle = await GenerateUniqueHandleAsync(baseHandle);

            user = new User
            {
                Handle = handle,
                DisplayName = string.IsNullOrWhiteSpace(displayName) ? handle : displayName!.Trim(),
                Email = email,
                ExternalProvider = provider,
                ExternalSubject = subject,
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // dar o starter bundle do core ao user novo
            await _cardService.GrantCoreStarterBundleAsync(user.Id);

            return user;
        }

        /// <summary>
        /// Gera um handle único com sufixos numéricos se necessário (base, base2, base3...).
        /// </summary>
        private async Task<string> GenerateUniqueHandleAsync(string baseHandle)
        {
            const int maxRetries = 20;

            for (int i = 0; i < maxRetries; i++)
            {
                var handle = i == 0 ? baseHandle : $"{baseHandle}{i + 1}";
                var exists = await _db.Users.AnyAsync(u => u.Handle == handle);
                if (!exists)
                    return handle;
            }

            throw new InvalidOperationException("Could not generate a unique handle.");
        }
    }
}
