using NeuroNexusBackend.DTOs;

namespace NeuroNexusBackend.Services
{
    public interface ISpawnService
    {
        Task<bool> CatchAsync(long id);
        Task<bool> ClaimAsync(long id);
        Task<long> CreateAsync(SpawnCreateRequestDTO req);
        Task<NearbyResponseDTO> NearbyAsync(double lat, double lon, int radiusM);
    }
}