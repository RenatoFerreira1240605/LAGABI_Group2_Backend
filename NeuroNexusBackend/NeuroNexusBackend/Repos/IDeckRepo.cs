using NeuroNexusBackend.Models;

namespace NeuroNexusBackend.Repos
{
    public interface IDeckRepo
    {
        Task<Deck> CreateAsync(long userId, string name, IEnumerable<(long cardId, short qty)> cards, CancellationToken ct);
        Task<Deck?> GetAsync(long deckId, CancellationToken ct);
        Task<List<Deck>> ListByUserAsync(long userId, CancellationToken ct);
    }
}