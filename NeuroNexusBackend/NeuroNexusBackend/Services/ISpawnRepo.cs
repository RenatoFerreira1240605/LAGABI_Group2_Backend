
namespace NeuroNexusBackend.Services
{
    public interface ISpawnRepo
    {
        Task CreateAsync(object lat, object lon, object cardId, object expiresAt, CancellationToken ct);
    }
}