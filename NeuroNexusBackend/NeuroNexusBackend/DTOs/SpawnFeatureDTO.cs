namespace NeuroNexusBackend.DTOs
{

    /// <summary>
    /// Single spawn feature with geographic position and minimal metadata.
    /// </summary>
    public struct SpawnFeatureDTO
    {
        /// <summary>Spawn id (DB: BIGINT).</summary>
        public long Id { get; set; }

        /// <summary>Lifecycle state: active|claimed|expired|caught.</summary>
        public string Status { get; set; }

        /// <summary>Optional card id preview (may be null).</summary>
        public int? CardPreview { get; set; }

        /// <summary>Optional expiration (UTC).</summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>Latitude (WGS84).</summary>
        public double Lat { get; set; }

        /// <summary>Longitude (WGS84).</summary>
        public double Lon { get; set; }
    }
}
