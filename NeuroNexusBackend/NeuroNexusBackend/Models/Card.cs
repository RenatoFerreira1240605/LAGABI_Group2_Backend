using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeuroNexusBackend.Models
{
    /// <summary> Catalog card definition used to build decks and spawns.
    [Table("Cards")]
    public class Card
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; } // DB identity (bigint)

        [Required, StringLength(64)]
        public string Name { get; set; } = default!; // Display name, unique per Suit

        [Required] // One of: Analytical/Creative/Structured/Social (enforced in DB CHECK)
        public string Suit { get; set; } = default!;

        [Required] // One of: Common/Rare/Unique/Legendary (enforced in DB CHECK)
        public string Rarity { get; set; } = default!;

        [Range(1, 5)]
        public short Points { get; set; } // Value 1..5

        [StringLength(200)]
        public string? Ability { get; set; } // Human-readable rules text (optional)

        [Required, StringLength(32)]
        public string Trigger { get; set; } = "on_reveal"; // e.g., on_reveal, on_points, once_per_game, ...

        [Required, StringLength(32)]
        public string Effect { get; set; } = "none"; // e.g., draw, gain_points, reduce_burnout, ...

        public short? Amount { get; set; } // numeric delta (e.g., +1/-1), null if N/A

        [StringLength(16)]
        public string? Target { get; set; } // self/opponent/both/deck/hand/...

        public bool OncePerGame { get; set; } // true if limited-use effect

        /// <summary>
        /// Optional JSON payload with extra conditions/parameters.
        /// Stored as JSONB in PostgreSQL.
        /// </summary>
        public string? AbilityJson { get; set; }
    }

}
