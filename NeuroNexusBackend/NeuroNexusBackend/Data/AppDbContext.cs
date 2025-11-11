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
            // --- Enable PostGIS extension in the database (required for spatial ops) ---
            b.HasPostgresExtension("postgis");

            // =========================
            // Users
            // =========================
            b.Entity<User>(e =>
            {
                // PK is Guid generated in app layer (no DB-generation needed)
                e.HasKey(x => x.Id);

                // Unique handle for login/display purposes
                e.HasIndex(x => x.Handle).IsUnique();

                // Simple column comments to aid DB introspection
                e.Property(x => x.Id).HasComment("Primary key for Users (UUID).");
                e.Property(x => x.Handle).HasMaxLength(32).HasComment("Unique handle/nickname.");
                e.Property(x => x.CreatedAt).HasComment("Creation timestamp (UTC).");
            });

            // =========================
            // Cards (official catalog)
            // =========================
            b.Entity<Card>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Name).HasMaxLength(64);
                e.Property(x => x.Suit).HasMaxLength(32);    // Analytical|Creative|Structured|Social
                e.Property(x => x.Rarity).HasMaxLength(8);   // C|U|R|SR|UR
                e.Property(x => x.Points);
            });

            // =========================
            // Decks / DeckCards (join)
            // =========================
            b.Entity<Deck>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Name).HasMaxLength(64);
                e.Property(x => x.CreatedAt);
                // FK => Users (owner)
                e.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                // Index for queries "list decks by user"
                e.HasIndex(x => x.UserId);
            });

            b.Entity<DeckCard>(e =>
            {
                // Composite primary key (DeckId + CardId)
                e.HasKey(x => new { x.DeckId, x.CardId });

                // FK to Deck
                e.HasOne(x => x.Deck)
                    .WithMany(d => d.Cards)
                    .HasForeignKey(x => x.DeckId)
                    .OnDelete(DeleteBehavior.Cascade);

                // FK to Card (optional navigation)
                e.HasOne(x => x.Card)
                    .WithMany()
                    .HasForeignKey(x => x.CardId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Basic constraint mirroring data annotations
                e.Property(x => x.Qty);
            });

            // =========================
            // Spawns (georeferenced)
            // =========================
            b.Entity<Spawn>(e =>
            {
                e.HasKey(x => x.Id);

                // Enforce SRID 4326 (WGS84 lon/lat) for spatial column
                e.Property(x => x.Location)
                    .HasSrid(4326)
                    .HasComment("Geographic point WGS84 (lon,lat).");

                // Spatial index for fast proximity queries (GiST)
                e.HasIndex(x => x.Location).HasMethod("GIST");

                // Filter/scheduling indexes
                e.HasIndex(x => x.Status);
                e.HasIndex(x => x.ExpiresAt);

                e.Property(x => x.Status).HasMaxLength(16);
                e.Property(x => x.CardId);
                e.Property(x => x.CreatedAt);
            });

            // =========================
            // Workshop / Player-created cards
            // =========================
            b.Entity<UserCard>(e =>
            {
                e.HasKey(x => x.Id);
                e.HasIndex(x => x.OwnerId);
                e.HasIndex(x => x.Status);

                e.Property(x => x.Title).HasMaxLength(64);
                e.Property(x => x.Suit).HasMaxLength(32);
                e.Property(x => x.EffectText).HasMaxLength(140);
                e.Property(x => x.ArtworkUrl).HasMaxLength(512);
                e.Property(x => x.Status).HasMaxLength(16).HasDefaultValue("draft");
                e.Property(x => x.PowerBudget);
                e.Property(x => x.Version).HasDefaultValue(1);

                // Optional template reference (no FK required if templates are immutable)
                // If you prefer FK:
                // e.HasOne<CardTemplate>().WithMany().HasForeignKey(x => x.TemplateId).OnDelete(DeleteBehavior.Restrict);
            });

            b.Entity<CardTemplate>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Name).HasMaxLength(48);
                e.Property(x => x.Guidance).HasMaxLength(200);
                e.Property(x => x.MinPoints);
                e.Property(x => x.MaxPoints);
                e.Property(x => x.MaxPowerBudget);
            });

            b.Entity<CardReview>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Outcome).HasMaxLength(16); // approved|rejected
                e.Property(x => x.Note).HasMaxLength(500);
                // If you want FK constraints:
                // e.HasOne<UserCard>().WithMany().HasForeignKey(x => x.UserCardId).OnDelete(DeleteBehavior.Cascade);
                // e.HasOne<User>().WithMany().HasForeignKey(x => x.ReviewerId).OnDelete(DeleteBehavior.Restrict);
            });

            // =========================
            // DDA / Hidden MMR
            // =========================
            b.Entity<MmrRating>(e =>
            {
                e.HasKey(x => x.Id);

                // Hidden rating is unique per (UserId, Mode)
                e.HasIndex(x => new { x.UserId, x.Mode }).IsUnique();

                e.Property(x => x.Mode).HasMaxLength(32).HasDefaultValue("pvp1v1");
                e.Property(x => x.Rating).HasDefaultValue(1000);     // Elo baseline
                e.Property(x => x.Deviation).HasDefaultValue(350);   // uncertainty proxy
                e.Property(x => x.Volatility).HasDefaultValue(120);  // future use
                e.Property(x => x.UpdatedAt);
            });

            b.Entity<Match>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Mode).HasMaxLength(32).HasDefaultValue("pvp1v1");
                e.Property(x => x.Status).HasMaxLength(16).HasDefaultValue("queued");
                e.HasIndex(x => x.Status);
                e.Property(x => x.CreatedAt);
                e.Property(x => x.StartedAt);
                e.Property(x => x.EndedAt);
            });

            b.Entity<TelemetryEvent>(e =>
            {
                e.HasKey(x => x.Id);
                e.HasIndex(x => x.MatchId);
                e.Property(x => x.Kind).HasMaxLength(48);
                // Store payload as TEXT; serialization is handled in app layer
                e.Property(x => x.PayloadJson).HasColumnType("text");
                e.Property(x => x.Ts);
            });
        }
    }
}
