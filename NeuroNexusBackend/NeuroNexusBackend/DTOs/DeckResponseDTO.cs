using System.ComponentModel.DataAnnotations;

namespace NeuroNexusBackend.DTOs
{

    /// <summary>
    /// Lightweight deck projection used in listings.
    /// </summary>
    public struct DeckResponseDTO
    {
        /// <summary>Deck id (DB: BIGINT).</summary>
        public long Id { get; set; }

        /// <summary>Deck name.</summary>
        public string Name { get; set; }

        /// <summary>Creation timestamp (UTC).</summary>
        public DateTime CreatedAt { get; set; }
    }
}
