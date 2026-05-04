using BusRejserLibrary.Database;
using BusRejserLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace BusRejserLibrary.Repositories
{
	public class VisitedLocationRepository
	{
		private readonly BusPlanenDbContext _context;

		public VisitedLocationRepository(BusPlanenDbContext context)
		{
			_context = context;
		}

		public List<VisitedLocation> GetByUserId(int userId)
		{
			return _context.VisitedLocations
				.AsNoTracking()
				.Where(x => x.UserId == userId)
				.OrderByDescending(x => x.LastVisitedAt)
				.ToList();
		}

		public void UpsertFromTravelHistory(TravelHistory history)
		{
			var name = !string.IsNullOrWhiteSpace(history.City)
				? history.City.Trim()
				: history.Destination.Trim();

			var country = string.IsNullOrWhiteSpace(history.Country)
				? "Unknown"
				: history.Country.Trim();

			var region = string.IsNullOrWhiteSpace(history.Region)
				? "Unknown"
				: history.Region.Trim();

			var existing = _context.VisitedLocations.FirstOrDefault(x =>
				x.UserId == history.UserId &&
				x.Name == name &&
				x.Country == country &&
				x.Region == region);


			if (existing == null)
			{
				_context.VisitedLocations.Add(new VisitedLocation
				{
					UserId = history.UserId,
					Name = name,
					Country = country,
					Region = region,
					Municipality = history.Municipality,
					Latitude = history.Latitude,
					Longitude = history.Longitude,
					FirstVisitedAt = history.CompletedAt,
					LastVisitedAt = history.CompletedAt,
					VisitCount = 1
				});
			}
			else
			{
				existing.VisitCount++;
				existing.LastVisitedAt = history.CompletedAt;

				if (existing.FirstVisitedAt > history.CompletedAt)
					existing.FirstVisitedAt = history.CompletedAt;

				if (!existing.Latitude.HasValue && history.Latitude.HasValue)
					existing.Latitude = history.Latitude;

				if (!existing.Longitude.HasValue && history.Longitude.HasValue)
					existing.Longitude = history.Longitude;
			}

			_context.SaveChanges();
		}
	}
}