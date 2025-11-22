
namespace NeuroNexusBackend.Services
{
    public interface IExpansionService
    {
        Task PurchaseAsync(long userId, string expansionCode);
    }
}