using NeuroNexusBackend.DTOs;

namespace NeuroNexusBackend.Services
{
    public interface IDeckService
    {
        Task<bool> AddCardAsync(long userId, long deckId, long cardId, short qty);
        Task<long> CreateAsync(long userId, DeckCreateRequestDTO req);
        Task<List<DeckListDTO>> GetUserDecksAsync(long userId, long? deckId);
        Task<List<DeckResponseDTO>> ListAsync(long userId);
        Task<bool> RemoveCardAsync(long userId, long deckId, long cardId, short qty);
    }
}