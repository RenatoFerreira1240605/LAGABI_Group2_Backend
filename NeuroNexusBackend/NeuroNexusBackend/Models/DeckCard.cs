using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeuroNexusBackend.Models
{
    /// <summary>
    /// Join entity between Deck and Card, containing the quantity per card.
    /// Uses a composite key (DeckId, CardId) configured in the DbContext.
    /// </summary>
    [Table("DeckCards")]
    public class DeckCard
    {
        /// <summary>
        /// Deck identifier (part of composite PK).
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long DeckId { get; set; }

        /// <summary>
        /// Navigation to the deck (optional on write).
        /// </summary>
        public Deck? Deck { get; set; }

        /// <summary>
        /// Card identifier (part of composite PK).
        /// </summary>
        [Required]
        public long CardId { get; set; }

        /// <summary>
        /// Navigation to the card (optional on write).
        /// </summary>
        public Card? Card { get; set; }

        /// <summary>
        /// Number of copies of this card in the deck (1..4 typical cap).
        /// </summary>
        [Required]
        [Range(1, 4)]
        public short Qty { get; set; }
    }

}
