namespace NeuroNexusBackend.Services
{
    /// <summary>
    /// Spawn service: input validation and mapping to DTOs.
    /// </summary>
    public class SpawnService : ISpawnService
    {
        private readonly ISpawnRepo _spawns;
        public SpawnService(ISpawnRepo spawns) => _spawns = spawns;

        public async Task<Guid> CreateAsync(SpawnCreateRequest req, CancellationToken ct)
        {
            // Validate coordinates quickly (rough bounds).
            if (req.Lat is < -90 or > 90) throw new ArgumentOutOfRangeException(nameof(req.Lat));
            if (req.Lon is < -180 or > 180) throw new ArgumentOutOfRangeException(nameof(req.Lon));
            var s = await _spawns.CreateAsync(req.Lat, req.Lon, req.CardId, req.ExpiresAt, ct);
            return s.Id;
        }

        public async Task<NearbyResponse> NearbyAsync(double lat, double lon, int radiusM, CancellationToken ct)
        {
            if (radiusM <= 0) radiusM = 200;
            var list = await _spawns.NearbyAsync(lat, lon, radiusM, ct);
            var features = list.Select(s => new SpawnFeature(
                s.Id.ToString(), s.Status, s.CardId, s.ExpiresAt,
                s.Location.Y, s.Location.X)).ToList();
            return new NearbyResponse(features);
        }

        public Task<bool> ClaimAsync(Guid id, CancellationToken ct) => _spawns.ClaimAsync(id, ct);
        public Task<bool> CatchAsync(Guid id, CancellationToken ct) => _spawns.CatchAsync(id, ct);
    }
}
