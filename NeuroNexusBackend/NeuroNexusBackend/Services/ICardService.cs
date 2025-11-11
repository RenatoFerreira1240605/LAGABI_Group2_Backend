using NeuroNexusBackend.DTOs;

namespace NeuroNexusBackend.Services
{
    public interface ICardService
    {
        Task<List<CardRuntimeDTO>> SearchAsync(string? suit, string? rarity, string? trigger, string? effect, CancellationToken ct);
        Task UpsertManyAsync(IEnumerable<CardUpsertDTO> payload, CancellationToken ct);
    }
}