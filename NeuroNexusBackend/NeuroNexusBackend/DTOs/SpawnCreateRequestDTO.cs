using System.ComponentModel.DataAnnotations;

namespace NeuroNexusBackend.DTOs
{

    /// <summary>
    /// Request body to create a georeferenced spawn at (lat, lon).
    /// </summary>
    public struct SpawnCreateRequestDTO
    {
        /// <summary>Latitude in WGS84 (-90..90).</summary>
        [Range(-90, 90)]
        public double Lat { get; set; }

        /// <summary>Longitude in WGS84 (-180..180).</summary>
        [Range(-180, 180)]
        public double Lon { get; set; }

        /// <summary>Optional card to preview/grant at this spawn.</summary>
        [Range(1, int.MaxValue)]
        public long? CardId { get; set; }

        /// <summary>Optional expiration timestamp (UTC).</summary>
        public DateTime? ExpiresAt { get; set; }
    }
}
