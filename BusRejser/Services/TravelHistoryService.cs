using BusRejser.DTOs;
using BusRejserLibrary.Enums;
using BusRejserLibrary.Models;
using BusRejserLibrary.Repositories;

namespace BusRejser.Services
{
	public class TravelHistoryService
	{
		private readonly IBookingRepository _bookingRepository;
		private readonly TravelHistoryRepository _travelHistoryRepository;
		private readonly BadgeEngine _badgeEngine;

		public TravelHistoryService(
			IBookingRepository bookingRepository,
			TravelHistoryRepository travelHistoryRepository,
			BadgeEngine badgeEngine)
		{
			_bookingRepository = bookingRepository;
			_travelHistoryRepository = travelHistoryRepository;
			_badgeEngine = badgeEngine;
		}

		public List<TravelHistoryResponse> GetByUserId(int userId)
		{
			SyncCompletedTripsForUser(userId);

			return _travelHistoryRepository.GetByUserId(userId)
				.Select(x => new TravelHistoryResponse
				{
					TravelHistoryId = x.TravelHistoryId,
					RejseId = x.RejseId,
					BookingId = x.BookingId,
					CompletedAt = x.CompletedAt,
					Destination = x.Destination,
					Country = x.Country,
					City = x.City,
					Region = x.Region,
					Municipality = x.Municipality
				})
				.ToList();
		}

		public void SyncCompletedTripsForUser(int userId)
		{
			var bookings = _bookingRepository.GetCompletedPaidWithRejseByUserId(userId);

			foreach (var booking in bookings)
			{
				if (_travelHistoryRepository.Exists(userId, booking.RejseId))
					continue;

				var rejse = booking.Rejse;
				if (rejse == null)
					continue;

				_travelHistoryRepository.Create(new TravelHistory
				{
					UserId = userId,
					RejseId = rejse.RejseId,
					BookingId = booking.BookingId,
					CompletedAt = rejse.EndAt,

					Destination = rejse.Destination,
					Country = rejse.Country,
					City = rejse.City,
					Region = rejse.Region,
					Municipality = rejse.Municipality
				});
			}

			_badgeEngine.EvaluateUserBadges(userId);
		}
	}
}