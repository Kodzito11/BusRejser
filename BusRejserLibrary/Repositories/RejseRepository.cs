using BusRejserLibrary.Database;
using BusRejserLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace BusRejserLibrary.Repositories
{
	public class RejseRepository : IRejseRepository
	{
		private readonly BusPlanenDbContext _context;

		public RejseRepository(BusPlanenDbContext context)
		{
			_context = context;
		}

		public int Create(Rejse rejse)
		{
			if (rejse == null)
				throw new ArgumentNullException(nameof(rejse));

			_context.Rejser.Add(rejse);
			_context.SaveChanges();

			return rejse.RejseId;
		}

		public Rejse? GetById(int id)
		{
			return _context.Rejser
				.AsNoTracking()
				.FirstOrDefault(x => x.RejseId == id);
		}

		public List<Rejse> GetAll()
		{
			return _context.Rejser
				.AsNoTracking()
				.OrderBy(x => x.StartAt)
				.ToList();
		}

		public bool Delete(int id)
		{
			var rejse = _context.Rejser.FirstOrDefault(x => x.RejseId == id);
			if (rejse == null)
				return false;

			_context.Rejser.Remove(rejse);
			_context.SaveChanges();
			return true;
		}

		public bool Update(int id, Rejse rejse)
		{
			if (rejse == null)
				throw new ArgumentNullException(nameof(rejse));

			var existing = _context.Rejser.FirstOrDefault(x => x.RejseId == id);
			if (existing == null)
				return false;

			existing.Title = rejse.Title;
			existing.Destination = rejse.Destination;
			existing.Country = rejse.Country;
			existing.City = rejse.City;
			existing.StartAt = rejse.StartAt;
			existing.EndAt = rejse.EndAt;
			existing.Price = rejse.Price;
			existing.MaxSeats = rejse.MaxSeats;
			existing.BusId = rejse.BusId;
			existing.ShortDescription = rejse.ShortDescription;
			existing.Description = rejse.Description;
			existing.ImageUrl = rejse.ImageUrl;
			existing.IsFeatured = rejse.IsFeatured;
			existing.IsPublished = rejse.IsPublished;
			existing.Version++;

			_context.SaveChanges();
			return true;
		}

		public bool TryReserveSeats(int rejseId, int antalPladser)
		{
			const int maxRetries = 3;

			for (int attempt = 0; attempt < maxRetries; attempt++)
			{
				var rejse = _context.Rejser.FirstOrDefault(x => x.RejseId == rejseId);
				if (rejse == null)
					return false;

				if (rejse.BookedSeats + antalPladser > rejse.MaxSeats)
					return false;

				rejse.BookedSeats += antalPladser;
				rejse.Version++;

				try
				{
					_context.SaveChanges();
					return true;
				}
				catch (DbUpdateConcurrencyException)
				{
					_context.ChangeTracker.Clear();

					if (attempt == maxRetries - 1)
						return false;
				}
			}

			return false;
		}

		public bool ReleaseSeats(int rejseId, int antalPladser)
		{
			const int maxRetries = 3;

			for (int attempt = 0; attempt < maxRetries; attempt++)
			{
				var rejse = _context.Rejser.FirstOrDefault(x => x.RejseId == rejseId);
				if (rejse == null)
					return false;

				if (rejse.BookedSeats < antalPladser)
					return false;

				rejse.BookedSeats -= antalPladser;
				rejse.Version++;

				try
				{
					_context.SaveChanges();
					return true;
				}
				catch (DbUpdateConcurrencyException)
				{
					_context.ChangeTracker.Clear();

					if (attempt == maxRetries - 1)
						return false;
				}
			}

			return false;
		}
	}
}