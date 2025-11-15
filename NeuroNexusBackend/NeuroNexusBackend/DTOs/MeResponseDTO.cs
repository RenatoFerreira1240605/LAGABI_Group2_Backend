namespace NeuroNexusBackend.DTOs
{
    public struct MeResponseDTO
    {
        public long Id { get; set; }
        public string Handle { get; set; }
        public string? DisplayName { get; set; }
        public string? Email { get; set; }
        public string? Provider { get; set; }
    }
}
