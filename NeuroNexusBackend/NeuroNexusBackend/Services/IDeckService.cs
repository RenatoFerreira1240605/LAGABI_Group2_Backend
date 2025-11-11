
namespace NeuroNexusBackend.Services
{
    public interface IDeckService
    {
        Task<Guid> CreateAsync(Guid userId, DeckCreateRequest req, CancellationToken ct);
        Task<List<DeckResponse>> ListAsync(Guid userId, CancellationToken ct);
    }
}