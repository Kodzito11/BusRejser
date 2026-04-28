using Microsoft.EntityFrameworkCore;
using BusRejserLibrary.Models;

namespace BusRejserLibrary.Database
{
	public class BusPlanenDbContext : DbContext
	{
		public BusPlanenDbContext(DbContextOptions<BusPlanenDbContext> options)
			: base(options)
		{
		}

		public DbSet<User> Users => Set<User>();
		public DbSet<Rejse> Rejser => Set<Rejse>();
		public DbSet<Booking> Bookings => Set<Booking>();
		public DbSet<Bus> Buses => Set<Bus>();
		public DbSet<Facilitet> Faciliteter => Set<Facilitet>();
		public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
		public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
		public DbSet<TravelHistory> TravelHistories => Set<TravelHistory>();
		public DbSet<VisitedLocation> VisitedLocations => Set<VisitedLocation>();
		public DbSet<Badge> Badges => Set<Badge>();
		public DbSet<UserBadge> UserBadges => Set<UserBadge>();

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<User>().ToTable("users");
			modelBuilder.Entity<Rejse>().ToTable("rejse");
			modelBuilder.Entity<Booking>().ToTable("booking");
			modelBuilder.Entity<Bus>().ToTable("bus");
			modelBuilder.Entity<Facilitet>().ToTable("facilitet");
			modelBuilder.Entity<PasswordResetToken>().ToTable("password_reset_tokens");
			modelBuilder.Entity<RefreshToken>().ToTable("refresh_tokens");

			modelBuilder.Entity<User>().HasKey(x => x.UserId);
			modelBuilder.Entity<Rejse>().HasKey(x => x.RejseId);
			modelBuilder.Entity<Booking>().HasKey(x => x.BookingId);
			modelBuilder.Entity<Facilitet>().HasKey(x => x.Id);
			modelBuilder.Entity<PasswordResetToken>().HasKey(x => x.Id);
			modelBuilder.Entity<RefreshToken>().HasKey(x => x.Id);
			modelBuilder.Entity<TravelHistory>().HasKey(x => x.TravelHistoryId);
			modelBuilder.Entity<VisitedLocation>().HasKey(x => x.VisitedLocationId);
			modelBuilder.Entity<Badge>().HasKey(x => x.BadgeId);
			modelBuilder.Entity<UserBadge>().HasKey(x => x.UserBadgeId);

			modelBuilder.Entity<User>(entity =>
			{
				entity.Property(x => x.UserId)
					.ValueGeneratedOnAdd();

				entity.Property(x => x.FirstName)
					.HasMaxLength(100);

				entity.Property(x => x.LastName)
					.HasMaxLength(100);

				entity.Property(x => x.Email)
					.IsRequired()
					.HasMaxLength(255);

				entity.Property(x => x.PhoneNumber)
					.HasMaxLength(30);

				entity.Property(x => x.PasswordHash)
					.IsRequired()
					.HasMaxLength(512);

				entity.Property(x => x.CreatedAt)
					.IsRequired();

				entity.Property(x => x.UpdatedAt)
					.IsRequired();

				entity.HasIndex(x => x.Email)
					.IsUnique();
			});

			modelBuilder.Entity<PasswordResetToken>()
				.HasOne<User>()
				.WithMany()
				.HasForeignKey(x => x.UserId)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<RefreshToken>(entity =>
			{
				entity.Property(x => x.TokenHash)
					.IsRequired()
					.HasMaxLength(128);

				entity.Property(x => x.ReplacedByTokenHash)
					.HasMaxLength(128);

				entity.Property(x => x.CreatedAt)
					.IsRequired();

				entity.Property(x => x.ExpiresAt)
					.IsRequired();

				entity.HasIndex(x => x.TokenHash)
					.IsUnique();

				entity.HasIndex(x => x.UserId);

				entity.HasOne<User>()
					.WithMany()
					.HasForeignKey(x => x.UserId)
					.OnDelete(DeleteBehavior.Cascade);
			});

			modelBuilder.Entity<Booking>(entity =>
			{
				entity.Property(x => x.KundeNavn)
					.IsRequired()
					.HasMaxLength(200);

				entity.Property(x => x.KundeEmail)
					.IsRequired()
					.HasMaxLength(255);

				entity.Property(x => x.BookingReference)
					.IsRequired()
					.HasMaxLength(32);

				entity.Property(x => x.StripeSessionId)
					.HasMaxLength(255);

				entity.Property(x => x.StripePaymentIntentId)
					.HasMaxLength(255);

				entity.Property(x => x.TotalPrice)
					.HasPrecision(18, 2);

				entity.HasIndex(x => x.BookingReference)
					.IsUnique();

				entity.HasIndex(x => x.StripeSessionId)
					.IsUnique();

				entity.HasOne<User>()
					.WithMany()
					.HasForeignKey(x => x.UserId)
					.OnDelete(DeleteBehavior.SetNull);
			});

			modelBuilder.Entity<Rejse>()
				.Property(x => x.Version)
				.IsConcurrencyToken();

			modelBuilder.Entity<Bus>()
				.HasMany(x => x.Faceliteter)
				.WithMany()
				.UsingEntity<Dictionary<string, object>>(
					"bus_facilitet",
					j => j
						.HasOne<Facilitet>()
						.WithMany()
						.HasForeignKey("FacilitetId")
						.OnDelete(DeleteBehavior.Cascade),
					j => j
						.HasOne<Bus>()
						.WithMany()
						.HasForeignKey("BusId")
						.OnDelete(DeleteBehavior.Cascade),
					j =>
					{
						j.HasKey("BusId", "FacilitetId");
						j.ToTable("bus_facilitet");
					});

			modelBuilder.Entity<Badge>(entity =>
			{
				entity.Property(x => x.BadgeId)
					.IsRequired()
					.HasMaxLength(100);
				entity.Property(x => x.BadgeName)
					.IsRequired()
					.HasMaxLength(100);
				entity.Property(x => x.Description)
					.HasMaxLength(500);
				entity.Property(x => x.Country)
					.HasMaxLength(100);
				entity.Property(x => x.Region)
					.IsRequired()
					.HasMaxLength(100);
				entity.Property(x => x.Municipality)
					.IsRequired()
					.HasMaxLength(100);
			});

			modelBuilder.Entity<UserBadge>(entity =>
			{
				entity.HasKey(x => x.UserBadgeId);

				entity.HasOne(x => x.User)
					.WithMany(x => x.UserBadges)
					.HasForeignKey(x => x.UserId)
					.OnDelete(DeleteBehavior.Cascade);

				entity.HasOne(x => x.Badge)
					.WithMany(x => x.UserBadges)
					.HasForeignKey(x => x.BadgeId)
					.OnDelete(DeleteBehavior.Cascade);

				entity.HasIndex(x => new { x.UserId, x.BadgeId })
					.IsUnique();
			});

			modelBuilder.Entity<TravelHistory>(entity =>
			{
				entity.HasKey(x => x.TravelHistoryId);

				entity.HasOne(x => x.User)
					.WithMany()
					.HasForeignKey(x => x.UserId)
					.OnDelete(DeleteBehavior.Cascade);

				entity.HasOne(x => x.Rejse)
					.WithMany()
					.HasForeignKey(x => x.RejseId)
					.OnDelete(DeleteBehavior.Cascade);

				entity.HasOne(x => x.Booking)
					.WithMany()
					.HasForeignKey(x => x.BookingId)
					.OnDelete(DeleteBehavior.Cascade);

				entity.HasIndex(x => new { x.UserId, x.RejseId })
					.IsUnique();
			});
			modelBuilder.Entity<VisitedLocation>(entity =>
			{
				entity.HasKey(x => x.VisitedLocationId);

				entity.HasOne(x => x.User)
					.WithMany()
					.HasForeignKey(x => x.UserId)
					.OnDelete(DeleteBehavior.Cascade);

				entity.Property(x => x.Name)
					.IsRequired()
					.HasMaxLength(150);

				entity.Property(x => x.Country)
					.IsRequired()
					.HasMaxLength(100);

				entity.Property(x => x.Region)
					.IsRequired()
					.HasMaxLength(100);

				entity.Property(x => x.Municipality)
					.HasMaxLength(100);

				entity.HasIndex(x => new { x.UserId, x.Name, x.Country, x.Region })
					.IsUnique();
			});

		}
	}
}
