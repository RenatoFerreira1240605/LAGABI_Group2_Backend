
namespace NeuroNexusBackend.Services
{
    public interface IMmrService
    {
        Task EnsureAsync(long userId, string mode, CancellationToken ct);
        Task<int> GetAsync(long userId, string mode, CancellationToken ct);
        Task UpdateAfterMatchAsync(long p1, long p2, string mode, int winner, CancellationToken ct);
    }
}