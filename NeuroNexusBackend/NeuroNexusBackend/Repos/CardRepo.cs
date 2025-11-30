using Microsoft.EntityFrameworkCore;
using NeuroNexusBackend.Data;
using NeuroNexusBackend.DTOs;
using NeuroNexusBackend.Models;

namespace NeuroNexusBackend.Repos
{
    public class CardRepo : ICardRepo
    {
        private readonly AppDbContext _db;
        public CardRepo(AppDbContext db) => _db = db;

        public async Task UpsertManyAsync(IEnumerable<CardUpsertDTO> payload)
        {
            foreach (var c in payload)
            {
                var expansionCode = string.IsNullOrWhiteSpace(c.ExpansionCode)
            ? "core"
            : c.ExpansionCode.Trim();

                var expansion = await _db.Expansions
                    .SingleOrDefaultAsync(e => e.Code == expansionCode);

                if (expansion == null)
                {
                    throw new InvalidOperationException($"Expansion '{expansionCode}' does not exist.");
                }
                var existing = await _db.Cards
                    .FirstOrDefaultAsync(x => x.Name == c.Name && x.Suit == c.Suit);

                if (existing == null)
                {
                    existing = new Card
                    {
                        Name = c.Name,
                        Suit = c.Suit,
                        Rarity = c.Rarity,
                        Points = c.Points,
                        Ability = c.Ability,
                        // enums são mapeados por texto no PG (usamos string no model)
                        Trigger = c.Trigger,
                        Effect = c.Effect,
                        Amount = c.Amount,
                        Target = c.Target,
                        OncePerGame = c.OncePerGame,
                        AbilityJson = c.AbilityJson,
                        ExpansionId = expansion.Id,
                        FlavorText = c.FlavorText,
                        CreatedAt = DateTime.UtcNow,
                        OwnerId = null,            // oficial
                        Status = "official"
                    };
                    _db.Cards.Add(existing);
                }
                else
                {
                    existing.Rarity = c.Rarity;
                    existing.Points = c.Points;
                    existing.Ability = c.Ability;
                    existing.Trigger = c.Trigger;
                    existing.Effect = c.Effect;
                    existing.Amount = c.Amount;
                    existing.Target = c.Target;
                    existing.OncePerGame = c.OncePerGame;
                    existing.AbilityJson = c.AbilityJson;
                    existing.ExpansionId = expansion.Id;
                    existing.FlavorText = c.FlavorText;
                    existing.UpdatedAt = DateTime.UtcNow;
                }
            }
            await _db.SaveChangesAsync();
        }

        public async Task<List<CardRuntimeDTO>> QueryRuntimeAsync(string? suit, string? rarity, string? trigger, string? effect)
        {
            var q = _db.Cards.AsNoTracking()
                .Include(c => c.Expansion)
                .Select(x => new CardRuntimeDTO
                {
                    Id = x.Id,
                    Name = x.Name,
                    Suit = x.Suit,
                    Rarity = x.Rarity,
                    Points = x.Points,
                    Ability = x.Ability,
                    Trigger = x.Trigger,
                    Effect = x.Effect,
                    Amount = x.Amount,
                    Target = x.Target,
                    OncePerGame = x.OncePerGame,
                    AbilityJson = x.AbilityJson,
                    ExpansionCode = x.Expansion.Code,
                    FlavorText = x.FlavorText
                });

            if (!string.IsNullOrWhiteSpace(suit)) q = q.Where(c => c.Suit == suit);
            if (!string.IsNullOrWhiteSpace(rarity)) q = q.Where(c => c.Rarity == rarity);
            if (!string.IsNullOrWhiteSpace(trigger)) q = q.Where(c => c.Trigger == trigger);
            if (!string.IsNullOrWhiteSpace(effect)) q = q.Where(c => c.Effect == effect);

            return await q.OrderBy(c => c.Suit).ThenBy(c => c.Points).ThenBy(c => c.Name).ToListAsync();
        }
        public async Task<List<ExpansionDTO>> GetExpansionsAsync(long? userId)
        {
            var expansions = await _db.Expansions
                .AsNoTracking()
                .ToListAsync();

            HashSet<long> ownedIds = new();

            if (userId.HasValue)
            {
                ownedIds = (await _db.UserExpansions
                        .Where(ue => ue.UserId == userId.Value)
                        .Select(ue => ue.ExpansionId)
                        .ToListAsync())
                    .ToHashSet();
            }

            return expansions
                .Select(e => new ExpansionDTO
                {
                    Code = e.Code,
                    Name = e.Name,
                    IsCore = e.IsCore,
                    Owned = userId.HasValue && ownedIds.Contains(e.Id)
                })
                .ToList();
        }

        public async Task PurchaseExpansionAsync(long userId, string expansionCode)
        {
            var expansion = await _db.Expansions
                .SingleOrDefaultAsync(e => e.Code == expansionCode);

            if (expansion == null)
                throw new InvalidOperationException($"Expansion '{expansionCode}' does not exist.");

            if (expansion.IsCore)
                throw new InvalidOperationException("Core expansion cannot be purchased.");

            var alreadyOwned = await _db.UserExpansions
                .AnyAsync(ue => ue.UserId == userId && ue.ExpansionId == expansion.Id);

            if (alreadyOwned)
                return; // já tem, não faz nada (ou lança InvalidOperation se preferires)

            using var tx = await _db.Database.BeginTransactionAsync();

            _db.UserExpansions.Add(new UserExpansion
            {
                UserId = userId,
                ExpansionId = expansion.Id,
                PurchasedAt = DateTimeOffset.UtcNow
            });

            // buscar todas as cartas desta expansão
            var cards = await _db.Cards
                .Where(c => c.ExpansionId == expansion.Id)
                .ToListAsync();

            foreach (var card in cards)
            {
                const short toAdd = 1; // nº de cópias por carta quando compras a expansão

                var inv = await _db.UserCardInventory
                    .SingleOrDefaultAsync(i => i.UserId == userId && i.CardId == card.Id);

                if (inv == null)
                {
                    var finalQuantity = Math.Min((short)4, toAdd);

                    inv = new UserCardInventory
                    {
                        UserId = userId,
                        CardId = card.Id,
                        Quantity = finalQuantity
                    };

                    _db.UserCardInventory.Add(inv);
                }
                else
                {
                    var newQuantity = (short)(inv.Quantity + toAdd);
                    inv.Quantity = Math.Min((short)4, newQuantity);
                }
            }

            await _db.SaveChangesAsync();
            await tx.CommitAsync();
        }
        public async Task GrantCoreStarterBundleAsync(long userId)
        {
            // expansion core
            var coreExpansion = await _db.Expansions
                .SingleOrDefaultAsync(e => e.IsCore);

            if (coreExpansion == null)
                throw new InvalidOperationException("Core expansion not found.");

            // evitar dar starter duas vezes ao mesmo user
            var alreadyHasCoreCards = await _db.UserCardInventory
                .Join(
                    _db.Cards,
                    inv => inv.CardId,
                    c => c.Id,
                    (inv, c) => new { inv, c }
                )
                .AnyAsync(x => x.inv.UserId == userId && x.c.ExpansionId == coreExpansion.Id);

            if (alreadyHasCoreCards)
                return;

            // todas as cartas do core
            var coreCards = await _db.Cards
                .Where(c => c.ExpansionId == coreExpansion.Id)
                .ToListAsync();

            var suits = new[] { "Analytical", "Creative", "Structured", "Social" };

            // helper para escolher N aleatórias por naipe/raridade
            List<Card> PickByRarity(string rarity, int perSuitCount)
            {
                var result = new List<Card>();

                foreach (var suit in suits)
                {
                    var candidates = coreCards
                        .Where(c => c.Rarity == rarity && c.Suit == suit)
                        .OrderBy(_ => Guid.NewGuid())      // random
                        .Take(perSuitCount)
                        .ToList();

                    result.AddRange(candidates);
                }

                return result;
            }

            // escolher cartas
            var commons = PickByRarity("Common", 2); // 2 por naipe -> até 8 comuns
            var rares = PickByRarity("Rare", 2); // 2 por naipe -> até 8 raras
            var uniques = PickByRarity("Unique", 1); // 1 por naipe -> até 4 unique
            var legendaries = PickByRarity("Legendary", 1); // 1 por naipe -> até 4 legendary

            // mapear para (card, qty)
            var grant = new List<(Card card, short qty)>();

            grant.AddRange(commons.Select(c => (c, (short)4))); // 4 cópias de cada common
            grant.AddRange(rares.Select(c => (c, (short)2)));   // 2 cópias de cada rare
            grant.AddRange(uniques.Select(c => (c, (short)2))); // 2 cópias de cada unique
            grant.AddRange(legendaries.Select(c => (c, (short)1))); // 1 cópia de cada legendary

            // aplicar à UserCardInventory (clamp a 4 como no PurchaseExpansion)
            using var tx = await _db.Database.BeginTransactionAsync();

            foreach (var (card, qty) in grant)
            {
                if (qty <= 0) continue;

                var inv = await _db.UserCardInventory
                    .SingleOrDefaultAsync(i => i.UserId == userId && i.CardId == card.Id);

                if (inv == null)
                {
                    var finalQuantity = Math.Min((short)4, qty);
                    inv = new UserCardInventory
                    {
                        UserId = userId,
                        CardId = card.Id,
                        Quantity = finalQuantity
                    };
                    _db.UserCardInventory.Add(inv);
                }
                else
                {
                    var newQuantity = (short)(inv.Quantity + qty);
                    inv.Quantity = Math.Min((short)4, newQuantity);
                }
            }

            await _db.SaveChangesAsync();
            await tx.CommitAsync();
        }
        public async Task<List<UserCardDTO>> GetUserCollectionAsync(long userId)
        {
            var query =
                from inv in _db.UserCardInventory.AsNoTracking()
                join c in _db.Cards.AsNoTracking() on inv.CardId equals c.Id
                join e in _db.Expansions.AsNoTracking() on c.ExpansionId equals e.Id
                where inv.UserId == userId
                select new UserCardDTO
                {
                    CardId = c.Id,
                    Name = c.Name,
                    Suit = c.Suit,
                    Rarity = c.Rarity,
                    Points = c.Points,
                    Ability = c.Ability,
                    Trigger = c.Trigger,
                    Effect = c.Effect,
                    Amount = (short)c.Amount,
                    Target = c.Target,
                    OncePerGame = c.OncePerGame,
                    AbilityJson = c.AbilityJson,
                    Quantity = inv.Quantity,
                    ExpansionCode = e.Code,
                    ExpansionName = e.Name,
                    FlavorText = c.FlavorText
                };

            return await query.ToListAsync();
        }
        public async Task<List<WorkshopCardUpsertDTO>> GetWorkshopCardsAsync(long ownerId, string? status)
        {
            var q = _db.Cards
                .AsNoTracking()
                .Where(c => c.OwnerId == ownerId);

            if (!string.IsNullOrWhiteSpace(status))
                q = q.Where(c => c.Status == status);

            var result = await (
                from c in q
                join e in _db.Expansions.AsNoTracking()
                    on c.ExpansionId equals e.Id
                select new WorkshopCardUpsertDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                    Suit = c.Suit,
                    Rarity = c.Rarity,
                    Points = c.Points,
                    Ability = c.Ability,
                    Trigger = c.Trigger,
                    Effect = c.Effect,
                    Amount = c.Amount,
                    Target = c.Target,
                    OncePerGame = c.OncePerGame,
                    AbilityJson = c.AbilityJson,
                    FlavorText = c.FlavorText,
                    ExpansionCode = e.Code,
                    Status = c.Status
                }).ToListAsync();

            return result;
        }

        public async Task<WorkshopCardUpsertDTO?> GetWorkshopCardAsync(long ownerId, long cardId)
        {
            var q =
                from c in _db.Cards.AsNoTracking()
                join e in _db.Expansions.AsNoTracking()
                    on c.ExpansionId equals e.Id
                where c.Id == cardId && c.OwnerId == ownerId
                select new WorkshopCardUpsertDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                    Suit = c.Suit,
                    Rarity = c.Rarity,
                    Points = c.Points,
                    Ability = c.Ability,
                    Trigger = c.Trigger,
                    Effect = c.Effect,
                    Amount = c.Amount,
                    Target = c.Target,
                    OncePerGame = c.OncePerGame,
                    AbilityJson = c.AbilityJson,
                    FlavorText = c.FlavorText,
                    ExpansionCode = e.Code,
                    Status = c.Status
                };

            return await q.SingleOrDefaultAsync();
        }
        public async Task<ExpansionDTO> UpsertExpansionAsync(ExpansionUpsertDTO? dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            var code = dto.Code.Trim().ToLowerInvariant();
            var name = dto.Name.Trim();

            // opcional: garantir que só existe 1 core
            if (dto.IsCore)
            {
                var otherCoreExists = await _db.Expansions
                    .AnyAsync(e => e.IsCore && e.Code != code);

                if (otherCoreExists)
                    throw new InvalidOperationException("Only one core expansion is supported.");
            }

            var entity = await _db.Expansions
                .SingleOrDefaultAsync(e => e.Code == code);

            if (entity == null)
            {
                entity = new Expansion
                {
                    Code = code,
                    Name = name,
                    IsCore = dto.IsCore
                };

                _db.Expansions.Add(entity);
            }
            else
            {
                entity.Name = name;
                entity.IsCore = dto.IsCore;
            }

            await _db.SaveChangesAsync();

            return new ExpansionDTO
            {
                Code = entity.Code,
                Name = entity.Name,
                IsCore = entity.IsCore,
                Owned = false // este dto é genérico, o "Owned" é calculado noutro endpoint
            };
        }

        public async Task<WorkshopCardUpsertDTO> UpsertWorkshopCardAsync(long ownerId, WorkshopCardUpsertDTO dto)
        {
            var now = DateTime.UtcNow;

            // Sanity-check do status
            var status = (dto.Status ?? "draft").ToLowerInvariant();
            if (status != "draft" && status != "active")
                throw new ArgumentException("Status must be 'draft' or 'active'.");

            // Expansion
            var expansionCode = string.IsNullOrWhiteSpace(dto.ExpansionCode)
                ? "workshop"
                : dto.ExpansionCode;

            var expansion = await _db.Expansions
                .SingleAsync(e => e.Code == expansionCode);

            Card entity;
            if (dto.Id.HasValue && dto.Id.Value > 0)
            {
                // Update: só pode mexer na carta se for o dono
                entity = await _db.Cards
                    .FirstOrDefaultAsync(c => c.Id == dto.Id.Value && c.OwnerId == ownerId);

                if (entity == null)
                    throw new KeyNotFoundException("Card not found or not owned by user.");
            }
            else
            {
                // Create
                entity = new Card
                {
                    OwnerId = ownerId,
                    CreatedAt = now,
                };
                _db.Cards.Add(entity);
            }

            // Mapear campos editáveis
            entity.Name = dto.Name;
            entity.Suit = dto.Suit;
            entity.Rarity = dto.Rarity;
            entity.Points = dto.Points;
            entity.Ability = dto.Ability;
            entity.Trigger = dto.Trigger;
            entity.Effect = dto.Effect;
            entity.Amount = dto.Amount;
            entity.Target = dto.Target;
            entity.OncePerGame = dto.OncePerGame;
            entity.AbilityJson = dto.AbilityJson;
            entity.FlavorText = dto.FlavorText;
            entity.Status = status;
            entity.ExpansionId = expansion.Id;
            entity.UpdatedAt = now;

            await _db.SaveChangesAsync();

            // Devolver DTO normalizado
            return new WorkshopCardUpsertDTO
            {
                Id = entity.Id,
                Name = entity.Name,
                Suit = entity.Suit,
                Rarity = entity.Rarity,
                Points = entity.Points,
                Ability = entity.Ability,
                Trigger = entity.Trigger,
                Effect = entity.Effect,
                Amount = entity.Amount,
                Target = entity.Target,
                OncePerGame = entity.OncePerGame,
                AbilityJson = entity.AbilityJson,
                FlavorText = entity.FlavorText,
                ExpansionCode = expansion.Code,
                Status = entity.Status
            };
        }
    }
}
