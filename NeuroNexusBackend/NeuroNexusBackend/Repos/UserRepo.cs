using Microsoft.EntityFrameworkCore;
using NeuroNexusBackend.Data;
using NeuroNexusBackend.Models;

namespace NeuroNexusBackend.Repos
{

    /// <summary>
    /// EF Core implementation of IUserRepository.
    /// </summary>
    public class UserRepo : IUserRepo
    {
        private readonly AppDbContext _db;
        public UserRepo(AppDbContext db) => _db = db;

        public async Task<User> CreateGuestAsync(string handle)
        {
            // Create a guest user with a unique handle.
            var u = new User { Handle = handle };
            _db.Users.Add(u);
            await _db.SaveChangesAsync();
            return u;
        }

        public Task<User?> GetAsync(long id) =>
            _db.Users.FirstOrDefaultAsync(x => x.Id == id);

        public Task<User?> GetAsync(string email) =>
            _db.Users.FirstOrDefaultAsync(x => x.Email == email);
    }
}
