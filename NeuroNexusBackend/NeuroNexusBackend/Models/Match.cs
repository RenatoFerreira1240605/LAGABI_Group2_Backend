using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeuroNexusBackend.Models
{
    /// <summary>
    /// Minimal match record for fairness auditing and MMR updates.
    /// </summary>
    [Table("Matches")]
    [Index(nameof(Status))]
    public class Match
    {
        [Key] public Guid Id { get; set; } = Guid.NewGuid();

        [Required] public string Mode { get; set; } = "pvp1v1";

        [Required] public Guid Player1Id { get; set; }
        [Required] public Guid Player2Id { get; set; }

        /// <summary>queued|started|finished|abandoned</summary>
        [Required, RegularExpression("queued|started|finished|abandoned")]
        public string Status { get; set; } = "queued";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }

        /// <summary>Snapshot of ratings at start (for evenness metrics).</summary>
        public int? P1RatingStart { get; set; }
        public int? P2RatingStart { get; set; }

        /// <summary>Winner: 1 or 2 (null if abandoned).</summary>
        public int? Winner { get; set; }

        public short P1Points { get; set; }
        public short P2Points { get; set; }
    }
}
