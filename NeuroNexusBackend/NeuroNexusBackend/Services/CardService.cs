using Microsoft.AspNetCore.Cors.Infrastructure;
using NeuroNexusBackend.DTOs;
using NeuroNexusBackend.Repos;

namespace NeuroNexusBackend.Services
{
    public class CardService : ICardService
    {
        private readonly ICardRepo _repo;
        public CardService(ICardRepo repo) => _repo = repo;

        public Task UpsertManyAsync(IEnumerable<CardUpsertDTO> payload)
            => _repo.UpsertManyAsync(payload);

        public Task<List<CardRuntimeDTO>> SearchAsync(string? suit, string? rarity, string? trigger, string? effect)
            => _repo.QueryRuntimeAsync(suit, rarity, trigger, effect);
    }
}
