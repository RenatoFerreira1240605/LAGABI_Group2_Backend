namespace NeuroNexusBackend.DTOs
{
    public struct DevicePollResponseDTO
    {
        // "pending", "ok", "expired", "error"
        public string Status { get; set; }
        public string? SessionToken { get; set; }
        public string? DisplayName { get; set; }
        public string? Email { get; set; }
        public string? Message { get; set; }
    }
}
