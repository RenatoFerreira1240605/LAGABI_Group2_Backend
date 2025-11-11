using Microsoft.EntityFrameworkCore;
using NeuroNexusBackend.Models;

namespace NeuroNexusBackend.Data
{

    /// <summary>
    /// Entity Framework Core DbContext for Neuro Nexus.
    /// Configures all entities, keys, indexes and PostGIS settings.
    /// </summary>
    public class AppDbContext : DbContext
    {
        /// <summary>
        /// Standard constructor used by ASP.NET Core DI.
        /// </summary>
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // ===== DbSets (tables) =====
        // Core domain
        public DbSet<User> Users => Set<User>();
        public DbSet<Card> Cards => Set<Card>();
        public DbSet<Deck> Decks => Set<Deck>();
        public DbSet<DeckCard> DeckCards => Set<DeckCard>();
        public DbSet<Spawn> Spawns => Set<Spawn>();

        // Workshop (player-created cards)
        public DbSet<UserCard> UserCards => Set<UserCard>();
        public DbSet<CardTemplate> CardTemplates => Set<CardTemplate>();
        public DbSet<CardReview> CardReviews => Set<CardReview>();

        // DDA / Hidden MMR
        public DbSet<MmrRating> MmrRatings => Set<MmrRating>();
        public DbSet<Match> Matches => Set<Match>();
        public DbSet<TelemetryEvent> TelemetryEvents => Set<TelemetryEvent>();

        /// <summary>
        /// Fluent configuration for tables, indexes, constraints and PostGIS.
        /// </summary>
        protected override void OnModelCreating(ModelBuilder b)
        {
            
        }
    }
}
