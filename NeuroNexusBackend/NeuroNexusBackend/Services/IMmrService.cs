
namespace NeuroNexusBackend.Services
{
    public interface IMmrService
    {
        Task EnsureAsync(long userId, string mode);
        Task<int> GetAsync(long userId, string mode);
        Task UpdateAfterMatchAsync(long p1, long p2, string mode, int winner);
    }
}