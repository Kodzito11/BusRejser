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

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<User>().ToTable("users");
			modelBuilder.Entity<Rejse>().ToTable("rejse");
			modelBuilder.Entity<Booking>().ToTable("booking");
			modelBuilder.Entity<Bus>().ToTable("bus");
			modelBuilder.Entity<Facilitet>().ToTable("facilitet");
			modelBuilder.Entity<PasswordResetToken>().ToTable("password_reset_tokens");

			modelBuilder.Entity<User>().HasKey(x => x.UserId);
			modelBuilder.Entity<Rejse>().HasKey(x => x.RejseId);
			modelBuilder.Entity<Booking>().HasKey(x => x.BookingId);
			modelBuilder.Entity<Facilitet>().HasKey(x => x.Id);
			modelBuilder.Entity<PasswordResetToken>().HasKey(x => x.Id);

			modelBuilder.Entity<User>(entity =>
			{
				entity.Property(x => x.UserId)
					.ValueGeneratedOnAdd();

				entity.Property(x => x.Username)
					.IsRequired()
					.HasMaxLength(100);

				entity.Property(x => x.FullName)
					.HasMaxLength(200);

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

				entity.HasIndex(x => x.Username)
					.IsUnique();

				entity.HasIndex(x => x.Email)
					.IsUnique();
			});

			modelBuilder.Entity<PasswordResetToken>()
				.HasOne<User>()
				.WithMany()
				.HasForeignKey(x => x.UserId)
				.OnDelete(DeleteBehavior.Cascade);

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
		}
	}
}
