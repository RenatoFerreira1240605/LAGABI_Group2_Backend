using NeuroNexusBackend.Models;
using System.Threading.Tasks;

namespace NeuroNexusBackend.Repos
{
    public interface IDeckRepo
    {
        Task<bool> AddCardAsync(long userId, long deckId, long cardId, short qty, CancellationToken ct);
        Task<Deck> CreateAsync(long userId, string name, IEnumerable<(long cardId, short qty)> cards, CancellationToken ct);
        Task<Deck?> GetAsync(long deckId, CancellationToken ct);
        Task<List<Deck>> GetDecksWithCardsAsync(long userId, CancellationToken ct);
        Task<List<Deck>> ListByUserAsync(long userId, CancellationToken ct);
        Task<bool> RemoveCardAsync(long userId, long deckId, long cardId, short qty, CancellationToken ct);
        Task<List<Deck>> GetDecksWithCardsAsync(long userId, long deckId, CancellationToken ct);
    }
}