using System.ComponentModel.DataAnnotations;

namespace NeuroNexusBackend.DTOs
{
    /// <summary>
    /// Response payload for guest creation. Returned by POST /auth/guest.
    /// </summary>
    public struct GuestResponseDTO
    {
        /// <summary>Auto-increment user id (DB: BIGINT).</summary>
        [Required]
        public long UserId { get; set; }

        /// <summary>Public nickname of the user.</summary>
        [Required, StringLength(32, MinimumLength = 3)]
        public string Handle { get; set; }
    }
}
