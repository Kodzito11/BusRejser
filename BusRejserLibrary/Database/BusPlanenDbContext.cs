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


			modelBuilder.Entity<User>().HasKey(x => x.Id);
			modelBuilder.Entity<Rejse>().HasKey(x => x.RejseId);
			modelBuilder.Entity<Booking>().HasKey(x => x.BookingId);

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

			modelBuilder.Entity<Facilitet>().HasKey(x => x.Id);
			modelBuilder.Entity<PasswordResetToken>().HasKey(x => x.Id);
		}
	}
}