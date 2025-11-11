using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeuroNexusBackend.Models
{

    /// <summary>
    /// Moderation/review records for UserCard submissions.
    /// </summary>
    [Table("CardReviews")]
    public class CardReview
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserCardId { get; set; }

        [Required]
        public Guid ReviewerId { get; set; }  // could be an admin user

        [Required]
        public DateTime ReviewedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Outcome: approved|rejected.</summary>
        [Required, RegularExpression("approved|rejected")]
        public string Outcome { get; set; } = default!;

        /// <summary>Optional note explaining decision.</summary>
        [StringLength(500)]
        public string? Note { get; set; }
    }
}
