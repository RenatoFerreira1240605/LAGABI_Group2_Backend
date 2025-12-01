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

        public Task PurchaseExpansionAsync(long userId, string expansionCode)
            => _repo.PurchaseExpansionAsync(userId, expansionCode);
        public Task<List<ExpansionDTO>> GetExpansionsAsync(long? userId)
            => _repo.GetExpansionsAsync(userId);

        public Task GrantCoreStarterBundleAsync(long userId)
            => _repo.GrantCoreStarterBundleAsync(userId);

        public Task<List<UserCardDTO>> GetUserCollectionAsync(long userId)
            => _repo.GetUserCollectionAsync(userId);
        public Task<ExpansionDTO> UpsertExpansionAsync(ExpansionUpsertDTO dto)
            => _repo.UpsertExpansionAsync(dto);

        public Task<List<WorkshopCardUpsertDTO>> GetWorkshopCardsAsync(long userId, long? cardId, string? status)
            => _repo.GetWorkshopCardsAsync(userId, cardId, status);

        public Task<WorkshopCardUpsertDTO> UpsertWorkshopCardAsync(long userId, WorkshopCardUpsertDTO dto)
            => _repo.UpsertWorkshopCardAsync(userId, dto);
        public Task GrantCardsToUserAsync(long userId, IEnumerable<AddToInventoryDTO> grants)
            => _repo.GrantCardsToUserAsync(userId, grants);
    }
}
