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
                        ExpansionId = expansion.Id

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
                    existing.ExpansionId = expansion.Id
;
                }
            }
            await _db.SaveChangesAsync();
        }

        public async Task<List<CardRuntimeDTO>> QueryRuntimeAsync(string? suit, string? rarity, string? trigger, string? effect)
        {
            var q = _db.Cards.AsNoTracking().Select(x => new CardRuntimeDTO
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
                AbilityJson = x.AbilityJson
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

    }
}
