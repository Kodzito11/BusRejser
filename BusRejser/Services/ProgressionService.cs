using BusRejser.DTOs;
using BusRejserLibrary.Models;
using BusRejserLibrary.Repositories;

namespace BusRejser.Services
{
	public class ProgressionService
	{
		private readonly IBookingRepository _bookingRepository;
		private readonly TravelHistoryRepository _travelHistoryRepository;
		private readonly VisitedLocationRepository _visitedLocationRepository;
		private readonly BadgeEngine _badgeEngine;

		public ProgressionService(
			IBookingRepository bookingRepository,
			TravelHistoryRepository travelHistoryRepository,
			VisitedLocationRepository visitedLocationRepository,
			BadgeEngine badgeEngine)
		{
			_bookingRepository = bookingRepository;
			_travelHistoryRepository = travelHistoryRepository;
			_visitedLocationRepository = visitedLocationRepository;
			_badgeEngine = badgeEngine;
		}

		public void SyncUserProgress(int userId)
		{
			var completedBookings = _bookingRepository.GetCompletedPaidWithRejseByUserId(userId);

			foreach (var booking in completedBookings)
			{
				if (_travelHistoryRepository.ExistsByBookingId(userId, booking.BookingId))
					continue;

				var rejse = booking.Rejse;
				if (rejse == null)
					continue;

				var history = new TravelHistory
				{
					UserId = userId,
					RejseId = rejse.RejseId,
					BookingId = booking.BookingId,
					CompletedAt = rejse.EndAt,

					Destination = rejse.Destination,
					Country = rejse.Country,
					City = rejse.City,
					Region = rejse.Region,
					Municipality = rejse.Municipality,
					Latitude = rejse.Latitude,
					Longitude = rejse.Longitude
				};

				_travelHistoryRepository.Create(history);
				_visitedLocationRepository.UpsertFromTravelHistory(history);
			}

			_badgeEngine.EvaluateUserBadges(userId);
		}

		public ProgressionMapResponse GetMap(int userId)
		{
			SyncUserProgress(userId);

			var locations = _visitedLocationRepository.GetByUserId(userId);

			return new ProgressionMapResponse
			{
				VisitedLocationCount = locations.Count,

				VisitedCountryCount = locations
					.Where(x => !string.IsNullOrWhiteSpace(x.Country))
					.Select(x => x.Country.Trim().ToLowerInvariant())
					.Distinct()
					.Count(),

				Locations = locations.Select(x => new VisitedLocationMapResponse
				{
					VisitedLocationId = x.VisitedLocationId,
					Name = x.Name,
					Country = x.Country,
					Region = x.Region,
					Municipality = x.Municipality,
					Latitude = x.Latitude,
					Longitude = x.Longitude,
					VisitCount = x.VisitCount,
					FirstVisitedAt = x.FirstVisitedAt,
					LastVisitedAt = x.LastVisitedAt,
					HasCoordinates = x.Latitude.HasValue && x.Longitude.HasValue
				}).ToList(),

				Regions = locations
					.GroupBy(x => new
					{
						Country = string.IsNullOrWhiteSpace(x.Country)
							? "Unknown"
							: x.Country.Trim(),

						Region = string.IsNullOrWhiteSpace(x.Region)
							? "Unknown"
							: x.Region.Trim()
					})
					.Select(g => new RegionProgressResponse
					{
						Country = g.Key.Country,
						Region = g.Key.Region,
						VisitedLocationCount = g.Count(),
						TotalVisitCount = g.Sum(x => x.VisitCount),
						LastVisitedAt = g.Max(x => x.LastVisitedAt)
					})
					.OrderByDescending(x => x.LastVisitedAt)
					.ToList()
			};
		}
	}
}