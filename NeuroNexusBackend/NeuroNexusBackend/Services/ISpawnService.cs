using NeuroNexusBackend.DTOs;

namespace NeuroNexusBackend.Services
{
    public interface ISpawnService
    {
        Task<bool> CatchAsync(long id, CancellationToken ct);
        Task<bool> ClaimAsync(long id, CancellationToken ct);
        Task<long> CreateAsync(SpawnCreateRequestDTO req, CancellationToken ct);
        Task<NearbyResponseDTO> NearbyAsync(double lat, double lon, int radiusM, CancellationToken ct);
    }
}