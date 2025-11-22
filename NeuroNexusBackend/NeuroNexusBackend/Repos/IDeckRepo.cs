using NeuroNexusBackend.Models;
using System.Threading.Tasks;

namespace NeuroNexusBackend.Repos
{
    public interface IDeckRepo
    {
        Task<bool> AddCardAsync(long userId, long deckId, long cardId, short qty);
        Task<Deck> CreateAsync(long userId, string name, IEnumerable<(long cardId, short qty)> cards);
        Task<Deck?> GetAsync(long deckId);
        Task<List<Deck>> GetDecksWithCardsAsync(long userId);
        Task<List<Deck>> ListByUserAsync(long userId);
        Task<bool> RemoveCardAsync(long userId, long deckId, long cardId, short qty);
        Task<List<Deck>> GetDecksWithCardsAsync(long userId, long deckId);
    }
}