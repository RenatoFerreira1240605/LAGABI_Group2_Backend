using Microsoft.EntityFrameworkCore;
using NeuroNexusBackend.Data;
using NeuroNexusBackend.Models;

namespace NeuroNexusBackend.Repos
{

    /// <summary>
    /// EF Core implementation of IDeckRepository.
    /// </summary>
    public class DeckRepo : IDeckRepo
    {
        private readonly AppDbContext _db;
        public DeckRepo(AppDbContext db) => _db = db;

        public async Task<Deck> CreateAsync(Guid userId, string name, IEnumerable<(int cardId, short qty)> cards, CancellationToken ct)
        {
            // Create deck header
            var deck = new Deck { UserId = userId, Name = name };
            _db.Decks.Add(deck);
            await _db.SaveChangesAsync(ct);

            // Insert deck-card rows
            foreach (var (cardId, qty) in cards)
                _db.DeckCards.Add(new DeckCard { DeckId = deck.Id, CardId = cardId, Qty = qty });

            await _db.SaveChangesAsync(ct);
            return deck;
        }

        public Task<List<Deck>> ListByUserAsync(Guid userId, CancellationToken ct) =>
            _db.Decks.Where(x => x.UserId == userId)
                     .OrderByDescending(x => x.CreatedAt)
                     .AsNoTracking()
                     .ToListAsync(ct);

        public Task<Deck?> GetAsync(Guid deckId, CancellationToken ct) =>
            _db.Decks.Include(d => d.Cards)
                     .ThenInclude(dc => dc.Card)
                     .FirstOrDefaultAsync(d => d.Id == deckId, ct);
    }
}
