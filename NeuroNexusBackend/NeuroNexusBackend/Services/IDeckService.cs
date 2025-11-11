
using NeuroNexusBackend.DTOs;

namespace NeuroNexusBackend.Services
{
    public interface IDeckService
    {
        Task<long> CreateAsync(long userId, DeckCreateRequestDTO req, CancellationToken ct);
        Task<List<DeckResponseDTO>> ListAsync(long userId, CancellationToken ct);
    }
}