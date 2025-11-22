namespace NeuroNexusBackend.Models
{
    public class Expansion
    {
        public long Id { get; set; }
        public string Code { get; set; } = null!;  // "core", "expansao_1", ...
        public string Name { get; set; } = null!;
        public bool IsCore { get; set; }
    }
}
