using Microsoft.EntityFrameworkCore;
using NeuroNexusBackend.Data;
using NeuroNexusBackend.DTOs;
using NeuroNexusBackend.Models;
using NeuroNexusBackend.Repos;

namespace NeuroNexusBackend.Services
{
    /// <summary>Deck service: validate and orchestrate repository operations.</summary>
    public class DeckService : IDeckService
    {
        private readonly IDeckRepo _repo;
        public DeckService(IDeckRepo repo)
        {
            _repo = repo ?? throw new InvalidOperationException("IDeckRepo not resolved. Check DI registration.");
        }


        public async Task<long> CreateAsync(long userId, DeckCreateRequestDTO req, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(req.Name)) throw new ArgumentException("Name is required");
            var tuples = (req.Cards ?? new List<DeckCardDTO>())
                .Select(x => (x.CardId, (short)(x.Qty <= 0 ? 1 : x.Qty)));
            var deck = await _repo.CreateAsync(userId, req.Name, tuples, ct);


            return deck.Id;
        }

        public async Task<List<DeckResponseDTO>> ListAsync(long userId, CancellationToken ct)
        {
            var decks = await _repo.ListByUserAsync(userId, ct);
            return decks.Select(d => new DeckResponseDTO(d.Id, d.Name, d.CreatedAt)).ToList();
        }
        private readonly AppDbContext _db;


        public Task<bool> AddCardAsync(long userId, long deckId, long cardId, short qty, CancellationToken ct)
            => _repo.AddCardAsync(userId, deckId, cardId, qty, ct);

        public Task<bool> RemoveCardAsync(long userId, long deckId, long cardId, short qty, CancellationToken ct)
            => _repo.RemoveCardAsync(userId, deckId, cardId, qty, ct);

        public async Task<List<DeckListDTO>> GetUserDecksAsync(long userId, long? deckId, CancellationToken ct)
        {
            
            var decks = deckId==null? await _repo.GetDecksWithCardsAsync(userId, ct): 
                await _repo.GetDecksWithCardsAsync(userId, deckId.Value, ct);

            return decks.Select(d => new DeckListDTO
            {
                Id = d.Id,
                Name = d.Name,
                Cards = d.Cards
                    .OrderBy(c => c.CardId)
                    .Select(c => new DeckCardDTO
                    {
                        CardId = c.CardId,
                        Qty = c.Qty
                    })
                    .ToList()
            }).ToList();
        }
    }
}