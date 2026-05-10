using BusRejser.Features.Progression.Services;
using BusRejser.Features.TravelHistory.DTOs;
using BusRejserLibrary.Repositories;

namespace BusRejser.Features.TravelHistory.Services
{
	public class TravelHistoryService
	{
		private readonly TravelHistoryRepository _travelHistoryRepository;
		private readonly ProgressionService _progressionService;

		public TravelHistoryService(
			TravelHistoryRepository travelHistoryRepository,
			ProgressionService progressionService)
		{
			_travelHistoryRepository = travelHistoryRepository;
			_progressionService = progressionService;
		}

		public List<TravelHistoryResponse> GetByUserId(int userId)
		{
			_progressionService.SyncUserProgress(userId);

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
			_progressionService.SyncUserProgress(userId);
		}
	}
}