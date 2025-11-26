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
        public long Id { get; set; }

        [Required, StringLength(64)]
        public string Name { get; set; } = default!;

        [Required, StringLength(32)]
        public string Suit { get; set; } = default!;

        [Required, StringLength(16)]
        public string Rarity { get; set; } = default!;

        public short Points { get; set; }

        public string? Ability { get; set; }
        public string Trigger { get; set; } = default!;
        public string Effect { get; set; } = default!;
        public short? Amount { get; set; }
        public string? Target { get; set; }
        public bool OncePerGame { get; set; }
        public string? AbilityJson { get; set; }

        public long ExpansionId { get; set; }
        public Expansion Expansion { get; set; } = null!;

        // NOVO – workshop
        public long? OwnerId { get; set; }          // null = oficial, != null = user
        public string? FlavorText { get; set; }
        public string Status { get; set; } = "official";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }




}
