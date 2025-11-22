using NeuroNexusBackend.DTOs;

namespace NeuroNexusBackend.Services
{
    public interface IDeckService
    {
        Task<bool> AddCardAsync(long userId, long deckId, long cardId, short qty, CancellationToken ct);
        Task<long> CreateAsync(long userId, DeckCreateRequestDTO req, CancellationToken ct);
        Task<List<DeckListDTO>> GetUserDecksAsync(long userId, long? deckId, CancellationToken ct);
        Task<List<DeckResponseDTO>> ListAsync(long userId, CancellationToken ct);
        Task<bool> RemoveCardAsync(long userId, long deckId, long cardId, short qty, CancellationToken ct);
    }
}