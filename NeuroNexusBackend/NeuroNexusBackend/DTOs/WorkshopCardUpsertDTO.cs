using System.ComponentModel.DataAnnotations;

namespace NeuroNexusBackend.DTOs
{
    public struct WorkshopCardUpsertDTO
    {
        public long? Id { get; set; }

        [Required, StringLength(64)]
        public string Name { get; set; }

        [Required, StringLength(32)]
        public string Suit { get; set; }

        [Required, StringLength(16)]
        public string Rarity { get; set; }

        [Range(0, 5)]
        public short Points { get; set; }

        public string? Ability { get; set; }

        [Required]
        public string Trigger { get; set; }

        [Required]
        public string Effect { get; set; }

        public short? Amount { get; set; }
        public string? Target { get; set; }
        public bool OncePerGame { get; set; }

        public string? AbilityJson { get; set; }

        public string? FlavorText { get; set; }

        /// <summary>
        /// Código da expansão (podes usar "workshop" por omissão).
        /// </summary>
        public string? ExpansionCode { get; set; }

        /// <summary>
        /// "draft" ou "active".
        /// </summary>
        [Required]
        public string Status { get; set; }
    }
}
