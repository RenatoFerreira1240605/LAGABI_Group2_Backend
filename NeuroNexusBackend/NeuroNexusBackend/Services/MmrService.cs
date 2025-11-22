using Microsoft.EntityFrameworkCore;
using NeuroNexusBackend.Data;
using NeuroNexusBackend.Models;

namespace NeuroNexusBackend.Services
{

    /// <summary>Hidden MMR using simple Elo updates with variable K.</summary>
    public class MmrService : IMmrService
    {
        private readonly AppDbContext _db;
        public MmrService(AppDbContext db) => _db = db;

        public async Task<int> GetAsync(long userId, string mode)
        {
            var r = await _db.MmrRatings.FirstOrDefaultAsync(x => x.UserId == userId && x.Mode == mode);
            return r?.Rating ?? 1000;
        }

        public async Task EnsureAsync(long userId, string mode)
        {
            var r = await _db.MmrRatings.FirstOrDefaultAsync(x => x.UserId == userId && x.Mode == mode);
            if (r == null)
            {
                _db.MmrRatings.Add(new MmrRating { UserId = userId, Mode = mode, Rating = 1000 });
                await _db.SaveChangesAsync();
            }
        }

        public async Task UpdateAfterMatchAsync(long p1, long p2, string mode, int winner)
        {
            var r1 = await _db.MmrRatings.FirstOrDefaultAsync(x => x.UserId == p1 && x.Mode == mode);
            var r2 = await _db.MmrRatings.FirstOrDefaultAsync(x => x.UserId == p2 && x.Mode == mode);

            var createdR1 = false;
            var createdR2 = false;

            if (r1 == null) { r1 = new MmrRating { UserId = p1, Mode = mode, Rating = 1000 }; _db.MmrRatings.Add(r1); createdR1 = true; }
            if (r2 == null) { r2 = new MmrRating { UserId = p2, Mode = mode, Rating = 1000 }; _db.MmrRatings.Add(r2); createdR2 = true; }

            // expected scores
            double e1 = 1.0 / (1.0 + Math.Pow(10, (r2.Rating - r1.Rating) / 400.0));
            double e2 = 1.0 / (1.0 + Math.Pow(10, (r1.Rating - r2.Rating) / 400.0));

            // variable K
            int k1 = r1.Deviation > 250 ? 40 : 24;
            int k2 = r2.Deviation > 250 ? 40 : 24;

            double s1 = winner == 1 ? 1 : 0;
            double s2 = winner == 2 ? 1 : 0;

            r1.Rating = (int)Math.Round(r1.Rating + k1 * (s1 - e1));
            r2.Rating = (int)Math.Round(r2.Rating + k2 * (s2 - e2));
            r1.Deviation = Math.Max(100, r1.Deviation - 10);
            r2.Deviation = Math.Max(100, r2.Deviation - 10);
            r1.UpdatedAt = r2.UpdatedAt = DateTime.UtcNow;

            if (!createdR1) _db.MmrRatings.Update(r1);
            if (!createdR2) _db.MmrRatings.Update(r2);

            await _db.SaveChangesAsync();
        }
    }
}
