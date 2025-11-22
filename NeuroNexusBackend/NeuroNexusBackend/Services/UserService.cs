using NeuroNexusBackend.Models;
using NeuroNexusBackend.Repos;

namespace NeuroNexusBackend.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepo _repo;

        public UserService(IUserRepo repo)
        {
            _repo = repo;
        }

        public async Task<User?> GetUserByEmail(string email)
        {
            return await _repo.GetAsync(email);
        }
        public async Task<User?> GetUserById(long userId)
        {
            return await _repo.GetAsync(userId);
        }
    }
}
