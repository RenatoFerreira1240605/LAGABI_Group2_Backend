using NeuroNexusBackend.Models;

namespace NeuroNexusBackend.Repos
{
    public interface IUserRepo
    {
        Task<User> CreateGuestAsync(string handle);
        Task<User?> GetAsync(long id);
        Task<User?> GetAsync(string email);
    }
}