
using NeuroNexusBackend.DTOs;

namespace NeuroNexusBackend.Services
{
    public interface IAuthService
    {
        Task<GuestResponseDTO> CreateGuestAsync(GuestRequestDTO req, CancellationToken ct);
    }
}