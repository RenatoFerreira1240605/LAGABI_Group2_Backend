namespace NeuroNexusBackend.DTOs
{
    public struct DeviceStartResponseDTO
    {
        public Guid LoginRequestId { get; set; }
        public string UserCode { get; set; }
        public string VerificationUrl { get; set; }
        public int ExpiresIn { get; set; }
        public int PollInterval { get; set; }
    }
}
