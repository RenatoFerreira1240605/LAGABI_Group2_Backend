using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using NeuroNexusBackend.Data;
using NeuroNexusBackend.Models;
using Npgsql;

namespace NeuroNexusBackend.Repos
{

    /// <summary>
    /// EF Core implementation of ISpawnRepository.
    /// Uses PostGIS proximity query (ST_DWithin) for nearby search.
    /// </summary>
    public class SpawnRepo : ISpawnRepo
    {
        private readonly AppDbContext _db;
        private readonly GeometryFactory _gf = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

        public SpawnRepo(AppDbContext db) => _db = db;

        public async Task<Spawn> CreateAsync(double lat, double lon, long? cardId, DateTime? expiresAt)
        {
            var p = _gf.CreatePoint(new Coordinate(lon, lat));
            var s = new Spawn { Location = p, CardId = cardId, ExpiresAt = expiresAt, Status = "active" };
            _db.Spawns.Add(s);
            await _db.SaveChangesAsync();
            return s; // <- devolve a entidade com Id preenchido
        }

        public async Task<List<Spawn>> NearbyAsync(double lat, double lon, int radiusM)
        {
            // Raw SQL with geography cast for distance in meters (fast and simple).
            const string sql = @"SELECT * FROM ""Spawns""
                            WHERE ""Status""='active'
                            AND (""ExpiresAt"" IS NULL OR ""ExpiresAt"" > now())
                            AND ST_DWithin(""Location"", public.ST_SetSRID(public.ST_MakePoint(@lon,@lat),4326)::geography, @r)";
            return await _db.Spawns.FromSqlRaw(sql,
                new NpgsqlParameter("lon", lon),
                new NpgsqlParameter("lat", lat),
                new NpgsqlParameter("r", radiusM)
            ).AsNoTracking().ToListAsync();
        }

        public async Task<bool> ClaimAsync(long id)
        {
            // Optimistic update to move active -> claimed
            var s = await _db.Spawns.FirstOrDefaultAsync(x => x.Id == id && x.Status == "active");
            if (s is null) return false;
            s.Status = "claimed";
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CatchAsync(long id)
        {
            // Allow catching from active or claimed
            var s = await _db.Spawns.FirstOrDefaultAsync(x => x.Id == id && (x.Status == "active" || x.Status == "claimed"));
            if (s is null) return false;
            s.Status = "caught";
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
