using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeuroNexusBackend.Models
{
    /// <summary>
    /// Hidden MMR per user per mode. Updated after each match.
    /// </summary>
    [Table("MmrRatings")]
    [Index(nameof(UserId), nameof(Mode), IsUnique = true)]
    public class MmrRating
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required] public long UserId { get; set; }

        /// <summary>Game mode id (e.g., "pvp1v1").</summary>
        [Required, StringLength(32)]
        public string Mode { get; set; } = "pvp1v1";

        /// <summary>Current rating (Elo baseline 1000).</summary>
        [Range(0, 5000)]
        public int Rating { get; set; } = 1000;

        /// <summary>Uncertainty proxy (optional, for future Glicko/TrueSkill).</summary>
        [Range(0, 350)]
        public int Deviation { get; set; } = 350;

        /// <summary>Volatility proxy (for future use).</summary>
        [Range(10, 400)]
        public int Volatility { get; set; } = 120;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
