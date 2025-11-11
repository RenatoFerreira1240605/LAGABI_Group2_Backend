using NeuroNexusBackend.Models;

namespace NeuroNexusBackend.Repos
{
    public interface ISpawnRepo
    {
        Task<bool> CatchAsync(long id, CancellationToken ct);
        Task<bool> ClaimAsync(long id, CancellationToken ct);
        Task<Spawn> CreateAsync(double lat, double lon, long? cardId, DateTime? expiresAt, CancellationToken ct);
        Task<List<Spawn>> NearbyAsync(double lat, double lon, int radiusM, CancellationToken ct);
    }
}
