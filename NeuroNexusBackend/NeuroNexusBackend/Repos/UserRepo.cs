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

        public async Task<User> CreateGuestAsync(string handle, CancellationToken ct)
        {
            // Create a guest user with a unique handle.
            var u = new User { Handle = handle };
            _db.Users.Add(u);
            await _db.SaveChangesAsync(ct);
            return u;
        }

        public Task<User?> GetAsync(Guid id, CancellationToken ct) =>
            _db.Users.FirstOrDefaultAsync(x => x.Id == id, ct);
    }
}
