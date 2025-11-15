using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeuroNexusBackend.Models
{

    /// <summary>
    /// Player account. Supports guest/dev users and future external auth (e.g., Google).
    /// </summary>
    [Table("Users")]
    public class User
    {
        /// <summary>Database identity (BIGINT autoincrement).</summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>Unique handle/username (e.g., "user_dev").</summary>
        [Required, StringLength(32)]
        public string Handle { get; set; } = default!;

        /// <summary>Optional display name shown in UI.</summary>
        [StringLength(64)]
        public string? DisplayName { get; set; }

        /// <summary>External IdP (e.g., "google") when using federated login.</summary>
        [StringLength(128)]
        public string? ExternalProvider { get; set; }

        /// <summary>External subject/identifier from the IdP.</summary>
        [StringLength(256)]
        public string? ExternalSubject { get; set; }

        /// <summary>User email (optional, used with external auth).</summary>
        [StringLength(256)]
        public string? Email { get; set; }

        /// <summary>Creation timestamp (UTC).</summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Last Login Timestamp (UTC).</summary>
        public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;

    }

}
