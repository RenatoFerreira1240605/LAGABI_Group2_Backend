using NeuroNexusBackend.Models;

namespace NeuroNexusBackend.Services
{
    public interface IUserService
    {
        Task<User?> GetUserByEmail(string email);
        Task<User?> GetUserById(long userId);
    }
}