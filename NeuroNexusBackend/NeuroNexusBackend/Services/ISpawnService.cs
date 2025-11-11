
namespace NeuroNexusBackend.Services
{
    public interface ISpawnService
    {
        Task<bool> CatchAsync(Guid id, CancellationToken ct);
        Task<bool> ClaimAsync(Guid id, CancellationToken ct);
        Task<Guid> CreateAsync(SpawnCreateRequest req, CancellationToken ct);
        Task<NearbyResponse> NearbyAsync(double lat, double lon, int radiusM, CancellationToken ct);
    }
}