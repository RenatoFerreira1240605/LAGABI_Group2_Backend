using NeuroNexusBackend.Models;

namespace NeuroNexusBackend.Repos
{
    public interface ISpawnRepo
    {
        Task<bool> CatchAsync(long id);
        Task<bool> ClaimAsync(long id);
        Task<Spawn> CreateAsync(double lat, double lon, long? cardId, DateTime? expiresAt);
        Task<List<Spawn>> NearbyAsync(double lat, double lon, int radiusM);
    }
}
