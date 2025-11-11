using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata; // UseIdentityByDefaultColumn
using NeuroNexusBackend.Models;

namespace NeuroNexusBackend.Data
{
    /// <summary>
    /// Entity Framework Core DbContext for Neuro Nexus.
    /// Configures all entities, keys, indexes and PostGIS settings.
    /// </summary>
    public class AppDbContext : DbContext
    {
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
            // ---- Enable PostGIS (required for spatial operations) ----
            b.HasPostgresExtension("postgis");

            // =========================
            // Users
            // =========================
            b.Entity<User>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).UseIdentityByDefaultColumn(); // BIGINT identity
                e.HasIndex(x => x.Handle).IsUnique();
                e.Property(x => x.Handle).HasMaxLength(32);
            });

            // =========================
            // Cards (official catalog)
            // =========================
            b.Entity<Card>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).UseIdentityByDefaultColumn();
                e.Property(x => x.Name).HasMaxLength(64);
                e.Property(x => x.Suit).HasMaxLength(32);   // Analytical|Creative|Structured|Social
                e.Property(x => x.Rarity).HasMaxLength(8);  // C|U|R|SR|UR
            });

            // =========================
            // Decks / DeckCards (join)
            // =========================
            b.Entity<Deck>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).UseIdentityByDefaultColumn();
                e.Property(x => x.Name).HasMaxLength(64);
                e.HasIndex(x => x.UserId);

                e.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Composite key for the join table (no identity here)
            b.Entity<DeckCard>(e =>
            {
                e.HasKey(x => new { x.DeckId, x.CardId });
                e.HasOne(x => x.Deck)
                    .WithMany(d => d.Cards)
                    .HasForeignKey(x => x.DeckId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Card)
                    .WithMany()
                    .HasForeignKey(x => x.CardId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // =========================
            // Spawns (georeferenced)
            // =========================
            b.Entity<Spawn>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).UseIdentityByDefaultColumn();

                // SRID 4326 (WGS84) e tipo Point
                e.Property(x => x.Location)
                 .HasColumnType("geometry(Point,4326)");

                // Índice espacial GiST
                e.HasIndex(x => x.Location).HasMethod("GIST");

                e.HasIndex(x => x.Status);
                e.HasIndex(x => x.ExpiresAt);
                e.Property(x => x.Status).HasMaxLength(16);
            });

            // =========================
            // Workshop / Player-created cards
            // =========================
            b.Entity<UserCard>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).UseIdentityByDefaultColumn();
                e.HasIndex(x => x.OwnerId);
                e.HasIndex(x => x.Status);

                e.Property(x => x.Title).HasMaxLength(64);
                e.Property(x => x.Suit).HasMaxLength(32);
                e.Property(x => x.EffectText).HasMaxLength(140);
                e.Property(x => x.ArtworkUrl).HasMaxLength(512);
                e.Property(x => x.Status).HasMaxLength(16).HasDefaultValue("draft");
                e.Property(x => x.Version).HasDefaultValue(1);

                // Optional FK to CardTemplate (uncomment if you want the FK enforced)
                // e.HasOne<CardTemplate>().WithMany().HasForeignKey(x => x.TemplateId).OnDelete(DeleteBehavior.Restrict);
            });

            b.Entity<CardTemplate>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).UseIdentityByDefaultColumn();
                e.Property(x => x.Name).HasMaxLength(48);
                e.Property(x => x.Guidance).HasMaxLength(200);
            });

            b.Entity<CardReview>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).UseIdentityByDefaultColumn();
                e.Property(x => x.Outcome).HasMaxLength(16);
                e.Property(x => x.Note).HasMaxLength(500);

                // Optional FKs (uncomment if you want referential integrity here)
                // e.HasOne<UserCard>().WithMany().HasForeignKey(x => x.UserCardId).OnDelete(DeleteBehavior.Cascade);
                // e.HasOne<User>().WithMany().HasForeignKey(x => x.ReviewerId).OnDelete(DeleteBehavior.Restrict);
            });

            // =========================
            // DDA / Hidden MMR
            // =========================
            b.Entity<MmrRating>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).UseIdentityByDefaultColumn();

                e.HasIndex(x => new { x.UserId, x.Mode }).IsUnique();

                e.Property(x => x.Mode).HasMaxLength(32).HasDefaultValue("pvp1v1");
                e.Property(x => x.Rating).HasDefaultValue(1000);
                e.Property(x => x.Deviation).HasDefaultValue(350);
                e.Property(x => x.Volatility).HasDefaultValue(120);
            });

            b.Entity<Match>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).UseIdentityByDefaultColumn();

                e.HasIndex(x => x.Status);
                e.Property(x => x.Mode).HasMaxLength(32).HasDefaultValue("pvp1v1");
                e.Property(x => x.Status).HasMaxLength(16).HasDefaultValue("queued");
            });

            b.Entity<TelemetryEvent>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).UseIdentityByDefaultColumn();

                e.HasIndex(x => x.MatchId);
                e.HasIndex(x => x.UserId);

                e.Property(x => x.Kind).HasMaxLength(48);
                e.Property(x => x.PayloadJson).HasColumnType("text");
            });
        }
    }
}