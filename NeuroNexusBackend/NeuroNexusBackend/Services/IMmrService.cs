
namespace NeuroNexusBackend.Services
{
    public interface IMmrService
    {
        Task EnsureAsync(Guid userId, string mode, CancellationToken ct);
        Task<int> GetAsync(Guid userId, string mode, CancellationToken ct);
        Task UpdateAfterMatchAsync(Guid p1, Guid p2, string mode, int winner, CancellationToken ct);
    }
}