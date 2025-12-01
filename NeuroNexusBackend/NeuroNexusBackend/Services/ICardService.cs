using NeuroNexusBackend.DTOs;

namespace NeuroNexusBackend.Services
{
    public interface ICardService
    {
        Task<List<ExpansionDTO>> GetExpansionsAsync(long? userId);
        Task<List<UserCardDTO>> GetUserCollectionAsync(long userId);
        Task<List<WorkshopCardUpsertDTO>> GetWorkshopCardsAsync(long userId, long? cardId, string? status);
        Task GrantCardsToUserAsync(long userId, IEnumerable<AddToInventoryDTO> grants);
        Task GrantCoreStarterBundleAsync(long userId);
        Task PurchaseExpansionAsync(long userId, string expansionCode);
        Task<List<CardRuntimeDTO>> SearchAsync(string? suit, string? rarity, string? trigger, string? effect);
        Task<ExpansionDTO> UpsertExpansionAsync(ExpansionUpsertDTO dto);
        Task UpsertManyAsync(IEnumerable<CardUpsertDTO> payload);
        Task<WorkshopCardUpsertDTO> UpsertWorkshopCardAsync(long userId, WorkshopCardUpsertDTO dto);
    }
}