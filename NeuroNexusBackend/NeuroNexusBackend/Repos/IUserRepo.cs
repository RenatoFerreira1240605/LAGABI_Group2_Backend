using NeuroNexusBackend.Models;

namespace NeuroNexusBackend.Repos
{
    public interface IUserRepo
    {
        Task<User> CreateGuestAsync(string handle, CancellationToken ct);
        Task<User?> GetAsync(Guid id, CancellationToken ct);
    }
}