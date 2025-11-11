namespace NeuroNexusBackend.DTOs
{


    /// <summary>
    /// Nearby spawns response: compact list of features.
    /// </summary>
    public struct NearbyResponseDTO
    {
        /// <summary>Collection of nearby spawn features.</summary>
        public List<SpawnFeatureDTO> Features { get; set; }

        public NearbyResponseDTO(List<SpawnFeatureDTO> features)
        {
            Features = features;
        }
    }


}
