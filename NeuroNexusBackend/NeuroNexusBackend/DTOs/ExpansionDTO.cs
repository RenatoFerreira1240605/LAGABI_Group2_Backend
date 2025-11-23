namespace NeuroNexusBackend.DTOs
{
    public struct ExpansionDTO
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public bool IsCore { get; set; }
        public bool Owned { get; set; }   // true se o user já comprou
    }
}
