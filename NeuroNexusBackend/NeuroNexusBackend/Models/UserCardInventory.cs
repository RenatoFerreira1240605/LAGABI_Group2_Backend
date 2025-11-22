namespace NeuroNexusBackend.Models
{
    public class UserCardInventory
    {
        public long UserId { get; set; }
        public long CardId { get; set; }
        public short Quantity { get; set; }

        public User User { get; set; } = null!;
        public Card Card { get; set; } = null!;
    }
}
