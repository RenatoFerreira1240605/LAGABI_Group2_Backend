namespace NeuroNexusBackend.DTOs
{
    public struct UserCardDTO
    {
        public long CardId { get; set; }
        public string Name { get; set; }
        public string Suit { get; set; }
        public string Rarity { get; set; }
        public int Points { get; set; }
        public string Ability { get; set; }
        public string Trigger { get; set; }
        public string Effect { get; set; }
        public int Amount { get; set; }
        public string Target { get; set; }
        public bool OncePerGame { get; set; }
        public string? AbilityJson { get; set; }

        public short Quantity { get; set; }

        public string ExpansionCode { get; set; }
        public string ExpansionName { get; set; }
        public string? FlavorText { get; set; }
    }
}
