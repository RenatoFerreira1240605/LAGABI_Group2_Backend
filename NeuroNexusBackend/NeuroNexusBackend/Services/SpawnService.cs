using NeuroNexusBackend.DTOs;
using NeuroNexusBackend.Repos;

namespace NeuroNexusBackend.Services
{
    /// <summary>Spawn service: input validation and mapping to DTOs.</summary>
    public class SpawnService : ISpawnService
    {
        private readonly ISpawnRepo _spawns;
        public SpawnService(ISpawnRepo spawns) => _spawns = spawns;

        public async Task<long> CreateAsync(SpawnCreateRequestDTO req, CancellationToken ct)
        {
            if (req.Lat < -90 || req.Lat > 90) throw new ArgumentOutOfRangeException(nameof(req.Lat));
            if (req.Lon < -180 || req.Lon > 180) throw new ArgumentOutOfRangeException(nameof(req.Lon));

            var s = await _spawns.CreateAsync(req.Lat, req.Lon, req.CardId, req.ExpiresAt, ct);
            return s.Id;
        }

        public async Task<NearbyResponseDTO> NearbyAsync(double lat, double lon, int radiusM, CancellationToken ct)
        {
            if (radiusM <= 0) radiusM = 200;

            var list = await _spawns.NearbyAsync(lat, lon, radiusM, ct);
            var features = new List<SpawnFeatureDTO>(list.Count);
            foreach (var s in list)
            {
                features.Add(new SpawnFeatureDTO
                {
                    Id = s.Id,
                    Status = s.Status,
                    CardPreview = s.CardId,  // long?
                    ExpiresAt = s.ExpiresAt,
                    Lat = s.Location.Y,
                    Lon = s.Location.X
                });
            }
            return new NearbyResponseDTO { Features = features };
        }

        public Task<bool> ClaimAsync(long id, CancellationToken ct)
            => _spawns.ClaimAsync(id, ct);

        public Task<bool> CatchAsync(long id, CancellationToken ct)
            => _spawns.CatchAsync(id, ct);
    }
}
