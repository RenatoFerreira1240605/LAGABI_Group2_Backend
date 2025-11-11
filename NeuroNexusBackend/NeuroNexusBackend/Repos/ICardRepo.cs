using NeuroNexusBackend.DTOs;

namespace NeuroNexusBackend.Repos
{
    public interface ICardRepo
    {
        Task<List<CardRuntimeDTO>> QueryRuntimeAsync(string? suit, string? rarity, string? trigger, string? effect, CancellationToken ct);
        Task UpsertManyAsync(IEnumerable<CardUpsertDTO> payload, CancellationToken ct);
    }
}