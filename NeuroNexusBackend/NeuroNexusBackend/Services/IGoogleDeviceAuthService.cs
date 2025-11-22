using NeuroNexusBackend.DTOs;

namespace NeuroNexusBackend.Services
{
    public interface IGoogleDeviceAuthService
    {
        Task<DevicePollResponseDTO> PollAsync(Guid loginRequestId);
        Task<DeviceStartResponseDTO> StartAsync(CancellationToken ct);
    }
}