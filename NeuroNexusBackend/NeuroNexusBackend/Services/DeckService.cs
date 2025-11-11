using NeuroNexusBackend.Repos;

namespace NeuroNexusBackend.Services
{
    /// <summary>
    /// Deck service: validate and orchestrate repository operations.
    /// </summary>
    public class DeckService : IDeckService
    {
        private readonly IDeckRepo _decks;
        public DeckService(IDeckRepo decks) => _decks = decks;

        public async Task<Guid> CreateAsync(Guid userId, DeckCreateRequest req, CancellationToken ct)
        {
            // Simple guardrails (name must exist; total cards > 0)
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

        public async Task<List<DeckResponse>> ListAsync(Guid userId, CancellationToken ct)
        {
            var list = await _decks.ListByUserAsync(userId, ct);
            return list.Select(d => new DeckResponse(d.Id, d.Name, d.CreatedAt)).ToList();
        }
    }
}