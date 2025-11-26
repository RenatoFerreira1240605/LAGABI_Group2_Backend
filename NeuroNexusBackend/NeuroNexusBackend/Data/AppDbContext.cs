using Microsoft.EntityFrameworkCore;
using NeuroNexusBackend.Models;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata; // UseIdentityByDefaultColumn
using System.Reflection.Emit;

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
        public DbSet<User> Users { get; set; } = default!;
        public DbSet<DeviceLoginRequest> DeviceLoginRequests { get; set; } = default!;
        public DbSet<Card> Cards { get; set; } = default!;
        public DbSet<Deck> Decks { get; set; } = default!;
        public DbSet<DeckCard> DeckCards { get; set; } = default!;
        public DbSet<Spawn> Spawns { get; set; } = default!;
        public DbSet<Expansion> Expansions { get; set; } = default!;
        public DbSet<UserCardInventory> UserCardInventory { get; set; } = default!;
        public DbSet<UserExpansion> UserExpansions { get; set; } = default!;


        // DDA / Hidden MMR
        public DbSet<MmrRating> MmrRatings { get; set; } = default!;
        public DbSet<Match> Matches { get; set; } = default!;
        public DbSet<TelemetryEvent> TelemetryEvents { get; set; } = default!;

        /// <summary>
        /// Fluent configuration for tables, indexes, constraints and PostGIS.
        /// </summary>
        protected override void OnModelCreating(ModelBuilder b)
        {
            // ===== Schema & extensions =====
            b.HasDefaultSchema("public");
            b.HasPostgresExtension("postgis");

            // =========================
            // Users
            // =========================
            b.Entity<User>(e =>
            {
                e.ToTable("Users");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).UseIdentityByDefaultColumn();     // BIGINT identity
                e.Property(x => x.Handle).HasMaxLength(32).IsRequired();
                e.HasIndex(x => x.Handle).IsUnique();

                e.Property(x => x.DisplayName).HasMaxLength(64);
                e.Property(x => x.ExternalProvider).HasMaxLength(128);
                e.Property(x => x.ExternalSubject).HasMaxLength(256);
                e.Property(x => x.Email).HasMaxLength(256);
                e.Property(x => x.CreatedAt).HasDefaultValueSql("now()");

                e.HasIndex(u => new { u.ExternalProvider, u.ExternalSubject })
                    .IsUnique()
                    .HasFilter("\"ExternalProvider\" IS NOT NULL AND \"ExternalSubject\" IS NOT NULL");

            });
            // =========================
            // Device Logins
            // =========================
            b.Entity<DeviceLoginRequest>()
                .HasIndex(d => d.DeviceCode);

            b.Entity<DeviceLoginRequest>()
                .HasIndex(d => d.Status);

            // =========================
            // Cards (official catalog)
            // =========================
            b.Entity<Card>(e =>
            {
                e.ToTable("Cards");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).UseIdentityByDefaultColumn();

                e.Property(x => x.Name).HasMaxLength(64).IsRequired();
                e.Property(x => x.Suit).HasMaxLength(32).IsRequired();
                e.Property(x => x.Rarity).HasMaxLength(16).IsRequired();
                e.Property(x => x.Points).HasDefaultValue((short)0);

                e.Property(x => x.Ability).HasMaxLength(64);
                e.Property(x => x.Trigger).HasMaxLength(48);
                e.Property(x => x.Effect).HasMaxLength(48);
                e.Property(x => x.Amount);
                e.Property(x => x.Target).HasMaxLength(24);
                e.Property(x => x.OncePerGame).HasDefaultValue(false);
                e.Property(x => x.AbilityJson).HasColumnType("text");
                e.Property(x => x.ExpansionId).IsRequired();

                e.HasOne(x => x.Expansion)
                    .WithMany()
                    .HasForeignKey(x => x.ExpansionId)
                    .OnDelete(DeleteBehavior.Restrict);

                // NOVO – workshop
                e.Property(x => x.OwnerId);
                e.Property(x => x.FlavorText).HasMaxLength(200);
                e.Property(x => x.Status).HasMaxLength(16).HasDefaultValue("official");
                e.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
                e.Property(x => x.UpdatedAt);

                e.HasIndex(x => x.OwnerId);
                e.HasIndex(x => x.Status);
            });


            // =========================
            // Decks
            // =========================
            b.Entity<Deck>(e =>
            {
                e.ToTable("Decks");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).UseIdentityByDefaultColumn();

                e.Property(x => x.Name).HasMaxLength(64).IsRequired();
                e.Property(x => x.CreatedAt).HasDefaultValueSql("now()");

                e.HasIndex(x => x.UserId);
                e.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // =========================
            // DeckCards (join)
            // =========================
            b.Entity<DeckCard>(e =>
            {
                e.ToTable("DeckCards");
                e.HasKey(x => new { x.DeckId, x.CardId });               // PK composta
                e.Property(x => x.Qty).HasDefaultValue((short)1);

                e.HasOne(x => x.Deck)
                    .WithMany(d => d.Cards)
                    .HasForeignKey(x => x.DeckId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Card)
                    .WithMany()
                    .HasForeignKey(x => x.CardId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasIndex(x => new { x.DeckId, x.CardId }).IsUnique();  // salvaguarda adicional
            });

            // =========================
            // Spawns (georeferenced)
            // =========================
            b.Entity<Spawn>(e =>
            {
                e.ToTable("Spawns");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).UseIdentityByDefaultColumn();

                // PostGIS Point SRID 4326
                e.Property(x => x.Location).HasColumnType("geometry(Point,4326)").IsRequired();

                e.Property(x => x.Status).HasMaxLength(16).HasDefaultValue("active");
                e.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
                e.HasIndex(x => x.Status);
                e.HasIndex(x => x.ExpiresAt);
                e.HasIndex(x => x.Location).HasMethod("GIST");

                e.HasOne<Card>()
                    .WithMany()
                    .HasForeignKey(x => x.CardId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            
            // =========================
            // DDA / Hidden MMR
            // =========================
            b.Entity<MmrRating>(e =>
            {
                e.ToTable("MmrRatings");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).UseIdentityByDefaultColumn();

                e.Property(x => x.Mode).HasMaxLength(32).HasDefaultValue("pvp1v1");
                e.Property(x => x.Rating).HasDefaultValue(1000);
                e.Property(x => x.Deviation).HasDefaultValue(350);
                e.Property(x => x.Volatility).HasDefaultValue(120);

                e.HasIndex(x => new { x.UserId, x.Mode }).IsUnique();

                e.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            b.Entity<Match>(e =>
            {
                e.ToTable("Matches");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).UseIdentityByDefaultColumn();

                e.Property(x => x.Mode).HasMaxLength(32).HasDefaultValue("pvp1v1");
                e.Property(x => x.Status).HasMaxLength(16).HasDefaultValue("queued");
                e.Property(x => x.CreatedAt).HasDefaultValueSql("now()");

                e.HasIndex(x => x.Status);

                e.HasOne<User>().WithMany().HasForeignKey(x => x.Player1Id).OnDelete(DeleteBehavior.Restrict);
                e.HasOne<User>().WithMany().HasForeignKey(x => x.Player2Id).OnDelete(DeleteBehavior.Restrict);
            });

            b.Entity<TelemetryEvent>(e =>
            {
                e.ToTable("TelemetryEvents");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).UseIdentityByDefaultColumn();

                e.Property(x => x.Kind).HasMaxLength(48).IsRequired();
                e.Property(x => x.PayloadJson).HasColumnType("text");
                e.Property(x => x.Ts).HasDefaultValueSql("now()");

                e.HasIndex(x => x.MatchId);
                e.HasIndex(x => x.UserId);
            });

            // =========================
            // Expansions
            // =========================
            b.Entity<Expansion>(e =>
            {
                e.ToTable("Expansions");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).UseIdentityByDefaultColumn();

                e.Property(x => x.Code).HasMaxLength(50).IsRequired();
                e.HasIndex(x => x.Code).IsUnique();

                e.Property(x => x.Name).HasMaxLength(100).IsRequired();
                e.Property(x => x.IsCore).HasDefaultValue(false);

                // índice opcional para garantir apenas um Core
                e.HasIndex(x => x.IsCore)
                    .HasDatabaseName("UX_Expansions_SingleCore")
                    .IsUnique()
                    .HasFilter("\"IsCore\" = TRUE");
            });
            // =========================
            // UserCardInventory (cartas oficiais por jogador)
            // =========================
            b.Entity<UserCardInventory>(e =>
            {
                e.ToTable("UserCardInventory");
                e.HasKey(x => new { x.UserId, x.CardId });

                e.Property(x => x.Quantity).HasDefaultValue((short)0);
                e.HasCheckConstraint("CK_UserCardInventory_Quantity", "\"Quantity\" BETWEEN 0 AND 4");

                e.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Card)
                    .WithMany()
                    .HasForeignKey(x => x.CardId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            // =========================
            // UserExpansions (DLC por utilizador)
            // =========================
            b.Entity<UserExpansion>(e =>
            {
                e.ToTable("UserExpansions");
                e.HasKey(x => new { x.UserId, x.ExpansionId });

                e.Property(x => x.PurchasedAt).HasDefaultValueSql("now()");

                e.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Expansion)
                    .WithMany()
                    .HasForeignKey(x => x.ExpansionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

    }
}