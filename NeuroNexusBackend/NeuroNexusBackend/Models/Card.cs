namespace NeuroNexusBackend.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Microsoft.EntityFrameworkCore;

    namespace NeuroNexus.Api.Models;

    /// <summary>
    /// Catalog card definition used to build decks and spawns.
    /// </summary>
    [Table("Cards")]
    public class Card
    {
        /// <summary>
        /// Primary key (stable integer id for a catalog item).
        /// </summary>
        [Key]
        [Comment("Primary key for Cards (catalog id).")]
        public int Id { get; set; }

        /// <summary>
        /// Card display name.
        /// </summary>
        [Required]
        [StringLength(64)]
        [Comment("Display name of the card.")]
        public string Name { get; set; } = default!;

        /// <summary>
        /// Suit taxonomy (Analytical|Creative|Structured|Social).
        /// Stored as text for flexibility; validated by regex.
        /// </summary>
        [Required]
        [RegularExpression("Analytical|Creative|Structured|Social")]
        [Comment("Suit taxonomy of the card.")]
        public string Suit { get; set; } = default!;

        /// <summary>
        /// Rarity code (C|U|R|SR|UR).
        /// </summary>
        [Required]
        [RegularExpression("C|U|R|SR|UR")]
        [Comment("Rarity code (C, U, R, SR, UR).")]
        public string Rarity { get; set; } = default!;

        /// <summary>
        /// Scoring value used by game logic (1..5).
        /// </summary>
        [Required]
        [Range(1, 5)]
        [Comment("Point value for the card (1..5).")]
        public short Points { get; set; }
    }

}
