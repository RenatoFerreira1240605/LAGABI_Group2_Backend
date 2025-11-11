using NeuroNexusBackend.DTOs;
using NeuroNexusBackend.Repos;

namespace NeuroNexusBackend.Services
{
    /// <summary>Deck service: validate and orchestrate repository operations.</summary>
    public class DeckService : IDeckService
    {
        private readonly IDeckRepo _decks;
        public DeckService(IDeckRepo decks) => _decks = decks;

        public async Task<long> CreateAsync(long userId, DeckCreateRequestDTO req, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(req.Name))
                throw new ArgumentException("Deck name is required");
            if (req.Cards is null || req.Cards.Count == 0)
                throw new ArgumentException("Deck must contain at least one card");

            var deck = await _decks.CreateAsync(
                userId,
                req.Name.Trim(),
                req.Cards.Select(c => (c.CardId, c.Qty)),
                ct);

            return deck.Id;
        }

        public async Task<List<DeckResponseDTO>> ListAsync(long userId, CancellationToken ct)
        {
            var list = await _decks.ListByUserAsync(userId, ct);
            var result = new List<DeckResponseDTO>(list.Count);
            foreach (var d in list)
                result.Add(new DeckResponseDTO(d.Id, d.Name, d.CreatedAt));
            return result;
        }
    }
}