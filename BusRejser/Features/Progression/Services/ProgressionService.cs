using BusRejserLibrary.Models;
using BusRejserLibrary.Repositories;
using BusRejser.Features.Progression.DTOs;
using BusRejser.Features.Badges.Services;
using BusRejser.Features.Geo.Services.Interfaces;

namespace BusRejser.Features.Progression.Services
{
	public class ProgressionService
	{
		private readonly IBookingRepository _bookingRepository;
		private readonly IGeoNormalizationService _geoNormalizationService;
		private readonly TravelHistoryRepository _travelHistoryRepository;
		private readonly VisitedLocationRepository _visitedLocationRepository;
		private readonly QuestProgressService _questProgressService;
		private readonly ProgressionMapBuilder _progressionMapBuilder;
		private readonly ProgressionSyncService _progressionSyncService;
		private readonly BadgeEngine _badgeEngine;

		public ProgressionService(
			IBookingRepository bookingRepository,
			IGeoNormalizationService geoNormalizationService,
			TravelHistoryRepository travelHistoryRepository,
			VisitedLocationRepository visitedLocationRepository,
			QuestProgressService questProgressService,
			ProgressionMapBuilder progressionMapBuilder,
			ProgressionSyncService progressionSyncService,
			BadgeEngine badgeEngine)
		{
			_bookingRepository = bookingRepository;
			_geoNormalizationService = geoNormalizationService;
			_travelHistoryRepository = travelHistoryRepository;
			_visitedLocationRepository = visitedLocationRepository;
			_questProgressService = questProgressService;
			_progressionMapBuilder = progressionMapBuilder;
			_progressionSyncService = progressionSyncService;
			_badgeEngine = badgeEngine;
		}

		public void SyncUserProgress(int userId)
		{
			_progressionSyncService.SyncUserProgress(userId);
		}

		public ProgressionMapResponse GetMap(int userId)
		{
			SyncUserProgress(userId);

			var locations = _visitedLocationRepository.GetByUserId(userId);

			return _progressionMapBuilder.Build(locations);
		}


		public List<QuestProgressResponse> GetQuests(int userId)
		{
			SyncUserProgress(userId);

			var locations = _visitedLocationRepository.GetByUserId(userId);
			var territories = _progressionMapBuilder.BuildTerritories(locations);
			var municipalities = _progressionMapBuilder.BuildMunicipalities(locations);

			return _questProgressService.BuildQuests(locations, territories, municipalities);
		}
	}
}