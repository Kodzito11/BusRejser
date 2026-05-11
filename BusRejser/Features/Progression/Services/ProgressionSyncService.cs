using BusRejser.Features.Badges.Services;
using BusRejser.Features.Geo.Services.Interfaces;
using TravelHistoryModel = BusRejserLibrary.Models.TravelHistory;
using BusRejserLibrary.Repositories;

namespace BusRejser.Features.Progression.Services
{
	public class ProgressionSyncService
	{
		private readonly IBookingRepository _bookingRepository;
		private readonly IGeoNormalizationService _geoNormalizationService;
		private readonly TravelHistoryRepository _travelHistoryRepository;
		private readonly VisitedLocationRepository _visitedLocationRepository;
		private readonly BadgeEngine _badgeEngine;

		public ProgressionSyncService(
			IBookingRepository bookingRepository,
			IGeoNormalizationService geoNormalizationService,
			TravelHistoryRepository travelHistoryRepository,
			VisitedLocationRepository visitedLocationRepository,
			BadgeEngine badgeEngine)
		{
			_bookingRepository = bookingRepository;
			_geoNormalizationService = geoNormalizationService;
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

				var normalizedGeo = _geoNormalizationService.Normalize(rejse.Destination);

				var history = new TravelHistoryModel
				{
					UserId = userId,
					RejseId = rejse.RejseId,
					BookingId = booking.BookingId,
					CompletedAt = rejse.EndAt,

					Destination = rejse.Destination,
					Country = normalizedGeo.Country,
					Region = normalizedGeo.Region,
					Municipality = normalizedGeo.Municipality,
					Latitude = normalizedGeo.Latitude,
					Longitude = normalizedGeo.Longitude
				};

				_travelHistoryRepository.Create(history);
				_visitedLocationRepository.UpsertFromTravelHistory(history);
			}

			_badgeEngine.EvaluateUserBadges(userId);
		}
	}
}