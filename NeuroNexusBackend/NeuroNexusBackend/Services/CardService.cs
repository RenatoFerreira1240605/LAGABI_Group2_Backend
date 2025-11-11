using Microsoft.AspNetCore.Cors.Infrastructure;
using NeuroNexusBackend.DTOs;
using NeuroNexusBackend.Repos;

namespace NeuroNexusBackend.Services
{
    public class CardService : ICardService
    {
        private readonly ICardRepo _repo;
        public CardService(ICardRepo repo) => _repo = repo;

        public Task UpsertManyAsync(IEnumerable<CardUpsertDTO> payload, CancellationToken ct)
            => _repo.UpsertManyAsync(payload, ct);

        public Task<List<CardRuntimeDTO>> SearchAsync(string? suit, string? rarity, string? trigger, string? effect, CancellationToken ct)
            => _repo.QueryRuntimeAsync(suit, rarity, trigger, effect, ct);
    }
}
