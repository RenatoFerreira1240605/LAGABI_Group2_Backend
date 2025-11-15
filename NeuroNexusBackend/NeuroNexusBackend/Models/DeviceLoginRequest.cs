using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeuroNexusBackend.Models
{
    [Table("DeviceLoginRequests")]
    public class DeviceLoginRequest
    {
        [Key]
        public Guid Id { get; set; }

        [Required, StringLength(256)]
        public string DeviceCode { get; set; } = default!;

        [Required, StringLength(64)]
        public string UserCode { get; set; } = default!;

        [Required, StringLength(256)]
        public string VerificationUrl { get; set; } = default!;

        public DateTime ExpiresAt { get; set; }

        public int IntervalSeconds { get; set; }

        // "Pending", "Completed", "Expired", "Error"
        [Required, StringLength(32)]
        public string Status { get; set; } = "Pending";

        // Ligação opcional ao utilizador quando o login termina
        public long? UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }
    }
}
