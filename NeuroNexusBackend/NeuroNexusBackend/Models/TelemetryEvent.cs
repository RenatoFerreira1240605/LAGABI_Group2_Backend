using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeuroNexusBackend.Models
{


    /// <summary>
    /// Generic event stream for analytics (fairness, queue times, etc.).
    /// </summary>
    [Table("TelemetryEvents")]
    [Index(nameof(MatchId))]
    public class TelemetryEvent
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public long? MatchId { get; set; }
        public long? UserId { get; set; }

        /// <summary>Event kind, e.g., "queue.enter", "queue.match", "turn.resolve".</summary>
        [Required, StringLength(48)]
        public string Kind { get; set; } = default!;

        /// <summary>Free-form JSON payload (serialized as text).</summary>
        public string? PayloadJson { get; set; }

        public DateTime Ts { get; set; } = DateTime.UtcNow;
    }
}
