using NeuroNexusBackend.DTOs;

namespace NeuroNexusBackend.Repos
{
    public interface ICardRepo
    {
        Task<List<ExpansionDTO>> GetExpansionsAsync(long? userId);
        Task<List<UserCardDTO>> GetUserCollectionAsync(long userId);
        Task<WorkshopCardUpsertDTO?> GetWorkshopCardAsync(long ownerId, long cardId);
        Task<List<WorkshopCardUpsertDTO>> GetWorkshopCardsAsync(long ownerId, string? status);
        Task GrantCoreStarterBundleAsync(long userId);
        Task PurchaseExpansionAsync(long userId, string expansionCode);
        Task<List<CardRuntimeDTO>> QueryRuntimeAsync(string? suit, string? rarity, string? trigger, string? effect);
        Task<ExpansionDTO> UpsertExpansionAsync(ExpansionUpsertDTO? dto);
        Task UpsertManyAsync(IEnumerable<CardUpsertDTO> payload);
        Task<WorkshopCardUpsertDTO> UpsertWorkshopCardAsync(long ownerId, WorkshopCardUpsertDTO dto);
    }
}