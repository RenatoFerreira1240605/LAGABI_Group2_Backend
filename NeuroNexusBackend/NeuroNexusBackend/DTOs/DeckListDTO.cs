namespace NeuroNexusBackend.DTOs
{
    public struct DeckListDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public List<DeckCardDTO> Cards { get; set; }
    }
}
