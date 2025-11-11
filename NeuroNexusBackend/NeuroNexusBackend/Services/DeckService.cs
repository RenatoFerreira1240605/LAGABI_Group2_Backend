using NeuroNexusBackend.DTOs;
using NeuroNexusBackend.Repos;

namespace NeuroNexusBackend.Services
{
    /// <summary>Deck service: validate and orchestrate repository operations.</summary>
    public class DeckService : IDeckService
    {
        private readonly IDeckRepo _repo;
        public async Task<long> CreateAsync(long userId, DeckCreateRequestDTO req, CancellationToken ct)
        {
            var tuples = req.Cards.Select(x => (x.CardId, (short)(x.Qty <= 0 ? 1 : x.Qty)));
            var deck = await _repo.CreateAsync(userId, req.Name, tuples, ct);
            return deck.Id;
        }

        public async Task<List<DeckResponseDTO>> ListAsync(long userId, CancellationToken ct)
        {
            var decks = await _repo.ListByUserAsync(userId, ct);
            return decks.Select(d => new DeckResponseDTO(d.Id, d.Name, d.CreatedAt)).ToList();
        }

        public Task<bool> AddCardAsync(long userId, long deckId, long cardId, short qty, CancellationToken ct)
            => _repo.AddCardAsync(userId, deckId, cardId, qty, ct);

        public Task<bool> RemoveCardAsync(long userId, long deckId, long cardId, short qty, CancellationToken ct)
            => _repo.RemoveCardAsync(userId, deckId, cardId, qty, ct);
    }
}