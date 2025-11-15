using NeuroNexusBackend.DTOs;
using NeuroNexusBackend.Models;

namespace NeuroNexusBackend.Services
{
    public interface IAuthService
    {
        Task<GuestResponseDTO> CreateGuestAsync(GuestRequestDTO req, CancellationToken ct);
        Task<User> GetOrCreateExternalUserAsync(string provider, string subject, string? email, string? displayName, CancellationToken ct);
    }
}