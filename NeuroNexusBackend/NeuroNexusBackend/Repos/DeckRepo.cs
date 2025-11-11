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

        public async Task<Deck> CreateAsync(long userId, string name, IEnumerable<(long cardId, short qty)> cards, CancellationToken ct)
        {
            var collapsed = cards
                .GroupBy(x => x.cardId)
                .Select(g => (cardId: g.Key, qty: (short)Math.Min(4, g.Sum(t => t.qty))))
                .Where(t => t.qty > 0)
                .ToList();

            var validIds = await _db.Cards
                .Where(c => collapsed.Select(t => t.cardId).Contains(c.Id))
                .Select(c => c.Id)
                .ToListAsync(ct);

            if (validIds.Count != collapsed.Count)
                throw new InvalidOperationException("One or more CardId do not exist.");

            var deck = new Deck { UserId = userId, Name = name };
            _db.Decks.Add(deck);
            await _db.SaveChangesAsync(ct);

            foreach (var (cardId, qty) in collapsed)
                _db.DeckCards.Add(new DeckCard { DeckId = deck.Id, CardId = cardId, Qty = qty });

            await _db.SaveChangesAsync(ct);
            return deck;
        }


        public Task<List<Deck>> ListByUserAsync(long userId, CancellationToken ct) =>
            _db.Decks.Where(x => x.UserId == userId)
                     .OrderByDescending(x => x.CreatedAt)
                     .AsNoTracking()
                     .ToListAsync(ct);

        public Task<Deck?> GetAsync(long deckId, CancellationToken ct) =>
            _db.Decks.Include(d => d.Cards)
                     .ThenInclude(dc => dc.Card)
                     .FirstOrDefaultAsync(d => d.Id == deckId, ct);

        public async Task<bool> AddCardAsync(long userId, long deckId, long cardId, short qty, CancellationToken ct)
        {
            if (qty <= 0) qty = 1;
            var deck = await _db.Decks.FirstOrDefaultAsync(d => d.Id == deckId && d.UserId == userId, ct);
            if (deck == null) return false;

            var exists = await _db.Cards.AnyAsync(c => c.Id == cardId, ct);
            if (!exists) return false;

            var row = await _db.DeckCards.FirstOrDefaultAsync(x => x.DeckId == deckId && x.CardId == cardId, ct);
            if (row == null) _db.DeckCards.Add(new DeckCard { DeckId = deckId, CardId = cardId, Qty = qty });
            else row.Qty = (short)Math.Min(4, row.Qty + qty); // exemplo: cap 4

            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> RemoveCardAsync(long userId, long deckId, long cardId, short qty, CancellationToken ct)
        {
            if (qty <= 0) qty = 1;
            var deck = await _db.Decks.FirstOrDefaultAsync(d => d.Id == deckId && d.UserId == userId, ct);
            if (deck == null) return false;

            var row = await _db.DeckCards.FirstOrDefaultAsync(x => x.DeckId == deckId && x.CardId == cardId, ct);
            if (row == null) return false;

            var newQty = row.Qty - qty;
            if (newQty > 0) row.Qty = (short)newQty;
            else _db.DeckCards.Remove(row);

            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}
