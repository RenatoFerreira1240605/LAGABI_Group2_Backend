using NeuroNexusBackend.DTOs;

namespace NeuroNexusBackend.Repos
{
    public interface ICardRepo
    {
        Task<List<CardRuntimeDTO>> QueryRuntimeAsync(string? suit, string? rarity, string? trigger, string? effect);
        Task UpsertManyAsync(IEnumerable<CardUpsertDTO> payload);
    }
}