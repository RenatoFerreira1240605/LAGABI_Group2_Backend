namespace NeuroNexusBackend.Models
{
    public class UserExpansion
    {

        public long UserId { get; set; }
        public long ExpansionId { get; set; }
        public DateTimeOffset PurchasedAt { get; set; }

        public User User { get; set; } = null!;
        public Expansion Expansion { get; set; } = null!;

    }
}
