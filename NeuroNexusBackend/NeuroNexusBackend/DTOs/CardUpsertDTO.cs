namespace NeuroNexusBackend.DTOs
{
    public struct CardUpsertDTO
    {
        public string Name { get; set; }          // Unique per Suit (de facto)
        public string Suit { get; set; }          // Analytical/Creative/Structured/Social
        public string Rarity { get; set; }        // Common/Rare/Unique/Legendary
        public short Points { get; set; }        // 1..5
        public string? Ability { get; set; }      // Texto descritivo
        public string Trigger { get; set; }       // enum text (ver DDL): on_reveal, on_points...
        public string Effect { get; set; }        // enum text: draw, gain_points, ...
        public short? Amount { get; set; }        // +1, -2, etc
        public string? Target { get; set; }       // self/opponent/both/deck/hand/...
        public bool OncePerGame { get; set; }     // true para “once per game”
        public string? AbilityJson { get; set; }  // condições extra arbitrárias
        public string? ExpansionCode { get; set; }
        public string? FlavorText { get; set; }
    }
}
