using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeuroNexusBackend.Models
{

    /// <summary>
    /// Baseline template for user-created cards (defines allowed ranges).
    /// </summary>
    [Table("CardTemplates")]
    public class CardTemplate
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(48)]
        public string Name { get; set; } = default!;

        /// <summary>Hard limits for Points.</summary>
        [Range(1, 5)]
        public short MinPoints { get; set; } = 1;

        [Range(1, 5)]
        public short MaxPoints { get; set; } = 5;

        /// <summary>Global budget ceiling for this template (0..100).</summary>
        [Range(0, 100)]
        public int MaxPowerBudget { get; set; } = 20;

        /// <summary>Optional guidance text shown in the workshop UI.</summary>
        [StringLength(200)]
        public string? Guidance { get; set; }
    }
}
