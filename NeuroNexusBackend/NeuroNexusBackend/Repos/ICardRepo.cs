using NeuroNexusBackend.DTOs;

namespace NeuroNexusBackend.Repos
{
    public interface ICardRepo
    {
        Task<List<ExpansionDTO>> GetExpansionsAsync(long? userId);
        Task PurchaseExpansionAsync(long userId, string expansionCode);
        Task<List<CardRuntimeDTO>> QueryRuntimeAsync(string? suit, string? rarity, string? trigger, string? effect);
        Task UpsertManyAsync(IEnumerable<CardUpsertDTO> payload);
    }
}