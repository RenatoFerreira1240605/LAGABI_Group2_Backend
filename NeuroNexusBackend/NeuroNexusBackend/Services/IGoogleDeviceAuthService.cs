using NeuroNexusBackend.DTOs;

namespace NeuroNexusBackend.Services
{
    public interface IGoogleDeviceAuthService
    {
        Task<DevicePollResponseDTO> PollAsync(Guid loginRequestId, CancellationToken ct);
        Task<DeviceStartResponseDTO> StartAsync(CancellationToken ct);
    }
}