using NeuroNexusBackend.Models;
using System.ComponentModel.DataAnnotations;

namespace NeuroNexusBackend.DTOs
{

    /// <summary>
    /// Request body to create a new deck owned by the caller.
    /// </summary>
    public struct DeckCreateRequestDTO
    {
        /// <summary>Human-friendly deck name.</summary>
        [Required, StringLength(64, MinimumLength = 1)]
        public string Name { get; set; }

        /// <summary>List of card entries (card id + quantity) to compose the deck.</summary>
        [Required, MinLength(1)]
        public List<DeckCardItemDTO> Cards { get; set; }
    }
}
