using Microsoft.EntityFrameworkCore;
using NeuroNexusBackend.Data;
using NeuroNexusBackend.Models;

namespace NeuroNexusBackend.Services
{
    public class ExpansionService : IExpansionService
    {
        private readonly AppDbContext _db;

        public ExpansionService(AppDbContext db)
        {
            _db = db;
        }

        public async Task PurchaseAsync(long userId, string expansionCode)
        {
            // 1) Obter a expansão
            var expansion = await _db.Expansions
                .SingleOrDefaultAsync(e => e.Code == expansionCode);

            if (expansion == null)
                throw new InvalidOperationException($"Expansion '{expansionCode}' não existe.");

            if (expansion.IsCore)
                throw new InvalidOperationException("A expansão 'core' não é comprável.");

            // 2) Verificar se o user já tem esta expansão
            var alreadyOwned = await _db.Set<UserExpansion>()
                .AnyAsync(ue => ue.UserId == userId && ue.ExpansionId == expansion.Id);

            if (alreadyOwned)
            {
                // Já tem, não faz nada (ou podes lançar erro se preferires)
                return;
            }

            using var tx = await _db.Database.BeginTransactionAsync();

            // 3) Registar a compra da expansão
            _db.Set<UserExpansion>().Add(new UserExpansion
            {
                UserId = userId,
                ExpansionId = expansion.Id,
                PurchasedAt = DateTimeOffset.UtcNow
            });

            // 4) Buscar todas as cartas desta expansão
            var cards = await _db.Cards
                .Where(c => c.ExpansionId == expansion.Id)
                .ToListAsync();

            foreach (var card in cards)
            {
                // Quantidade que esta expansão oferece por carta (ajusta se quiseres packs maiores)
                const short toAdd = 1;

                var inv = await _db.UserCardInventory
                    .SingleOrDefaultAsync(i => i.UserId == userId && i.CardId == card.Id);

                if (inv == null)
                {
                    // Novo registo no inventário
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
                    // Atualizar quantidade existente, respeitando o máximo de 4
                    var newQuantity = (short)(inv.Quantity + toAdd);
                    inv.Quantity = Math.Min((short)4, newQuantity);
                }
            }

            await _db.SaveChangesAsync();
            await tx.CommitAsync();
        }
    }

}
