using System.ComponentModel.DataAnnotations;

namespace NeuroNexusBackend.DTOs
{

    /// <summary>
    /// Single card entry inside a deck (card id + quantity).
    /// </summary>
    public struct DeckCardDTO
    {
        /// <summary>Catalog id of the card.</summary>
        [Range(1, int.MaxValue)]
        public long CardId { get; set; }

        /// <summary>Number of copies for this card in the deck (typical cap 1..4).</summary>
        [Range(1, 4)]
        public short Qty { get; set; }
    }
}
