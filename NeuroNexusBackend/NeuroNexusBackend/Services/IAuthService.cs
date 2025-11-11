
namespace NeuroNexusBackend.Services
{
    public interface IAuthService
    {
        Task<GuestResponse> CreateGuestAsync(GuestRequest req, CancellationToken ct);
    }
}