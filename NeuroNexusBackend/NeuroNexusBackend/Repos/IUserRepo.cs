using NeuroNexusBackend.Models;

namespace NeuroNexusBackend.Repos
{
    public interface IUserRepo
    {
        Task<User> CreateGuestAsync(string handle, CancellationToken ct);
        Task<User?> GetAsync(long id, CancellationToken ct);
    }
}