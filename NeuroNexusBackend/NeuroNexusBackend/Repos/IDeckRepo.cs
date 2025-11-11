using NeuroNexusBackend.Models;

namespace NeuroNexusBackend.Repos
{
    public interface IDeckRepo
    {
        Task<Deck> CreateAsync(Guid userId, string name, IEnumerable<(int cardId, short qty)> cards, CancellationToken ct);
        Task<Deck?> GetAsync(Guid deckId, CancellationToken ct);
        Task<List<Deck>> ListByUserAsync(Guid userId, CancellationToken ct);
    }
}