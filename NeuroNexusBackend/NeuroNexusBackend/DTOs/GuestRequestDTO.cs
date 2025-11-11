namespace NeuroNexusBackend.DTOs
{
    /// <summary>
    /// Request to create a guest user (optional handle).
    /// </summary>
    public struct GuestRequestDTO
    {
        /// <summary>Optional public nickname to use for the guest account.</summary>
        public string? Handle { get; set; }
    }
}
