namespace NeuroNexusBackend.DTOs
{
    public struct CardRuntimeDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Suit { get; set; }
        public string Rarity { get; set; }
        public short Points { get; set; }
        public string? Ability { get; set; }
        public string Trigger { get; set; }
        public string Effect { get; set; }
        public short? Amount { get; set; }
        public string? Target { get; set; }
        public bool OncePerGame { get; set; }
        public string? AbilityJson { get; set; }
        public string? ExpansionCode { get; set; }
    }
}
