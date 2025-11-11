using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeuroNexusBackend.Models
{
    /// <summary>
    /// Player-created card (DLC Workshop). Stored separately from official catalog.
    /// </summary>
    [Table("UserCards")]
    [Index(nameof(OwnerId))]
    [Index(nameof(Status))]
    public class UserCard
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Owner of this card design.</summary>
        [Required]
        public Guid OwnerId { get; set; }

        /// <summary>Human-friendly title of the card.</summary>
        [Required, StringLength(64)]
        public string Title { get; set; } = default!;

        /// <summary>Reference to a template (baseline rules/budget).</summary>
        public int? TemplateId { get; set; }

        /// <summary>Suit taxonomy like official cards.</summary>
        [Required, RegularExpression("Analytical|Creative|Structured|Social")]
        public string Suit { get; set; } = default!;

        /// <summary>Point value within allowed bounds (guardrail).</summary>
        [Required, Range(1, 5)]
        public short Points { get; set; }

        /// <summary>Budget consumed by special tags/effects (normalized 0..100).</summary>
        [Range(0, 100)]
        public int PowerBudget { get; set; } = 0;

        /// <summary>Optional short text describing the effect (declarative).</summary>
        [StringLength(140)]
        public string? EffectText { get; set; }

        /// <summary>Artwork URL on object storage (no binaries in DB for now).</summary>
        [StringLength(512)]
        public string? ArtworkUrl { get; set; }

        /// <summary>Workflow status: draft|submitted|approved|rejected.</summary>
        [Required, RegularExpression("draft|submitted|approved|rejected")]
        public string Status { get; set; } = "draft";

        /// <summary>Semantic version for edits across submissions (e.g., 1,2,3).</summary>
        [Range(1, int.MaxValue)]
        public int Version { get; set; } = 1;

        /// <summary>Moderation note (filled on review).</summary>
        [StringLength(500)]
        public string? ModerationNote { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}