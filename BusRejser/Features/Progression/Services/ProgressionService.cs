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
		private readonly BadgeEngine _badgeEngine;

		public ProgressionService(
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
				var history = new BusRejserLibrary.Models.TravelHistory
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
					.ToList(),

					Territories = BuildTerritories(locations),
					Municipalities = BuildMunicipalities(locations)
			};
		}
		private static List<TerritoryProgressResponse> BuildTerritories(List<VisitedLocation> locations)
		{
			var definitions = new[]
			{
		new { Key = "dk", Name = "Danmark", Type = "country", Matches = new[] { "danmark", "denmark", "dk" } },
		new { Key = "germany", Name = "Tyskland", Type = "country", Matches = new[] { "tyskland", "germany", "de" } },
		new { Key = "czechia", Name = "Tjekkiet", Type = "country", Matches = new[] { "tjekkiet", "czechia", "czech republic", "cz" } },
		new { Key = "netherlands", Name = "Holland", Type = "country", Matches = new[] { "holland", "netherlands", "nl" } },
		new { Key = "sweden", Name = "Sverige", Type = "country", Matches = new[] { "sverige", "sweden", "se" } },
		new { Key = "norway", Name = "Norge", Type = "country", Matches = new[] { "norge", "norway", "no" } }
	};

			return definitions.Select(def =>
			{
				var visitCount = locations
					.Where(x => def.Matches.Contains(Normalize(x.Country)) || def.Matches.Contains(Normalize(x.Name)))
					.Sum(x => x.VisitCount);

				return new TerritoryProgressResponse
				{
					Key = def.Key,
					Name = def.Name,
					Type = def.Type,
					VisitCount = visitCount,
					Status = ResolveStatus(visitCount),
					CompletionPercent = ResolveCompletionPercent(visitCount)
				};
			}).ToList();
		}

		private static string Normalize(string? value)
		{
			return (value ?? "")
				.Trim()
				.ToLowerInvariant();
		}

		private static string ResolveStatus(int visitCount)
		{
			if (visitCount >= 10) return "mastered";
			if (visitCount >= 1) return "unlocked";
			return "locked";
		}

		private static int ResolveCompletionPercent(int visitCount)
		{
			if (visitCount <= 0) return 0;
			if (visitCount >= 10) return 100;

			return visitCount * 10;
		}

		private static List<MunicipalityProgressResponse> BuildMunicipalities(List<VisitedLocation> locations)
		{
			return locations
				.Where(x => !string.IsNullOrWhiteSpace(x.Municipality))
				.GroupBy(x => new
				{
					Municipality = x.Municipality!.Trim(),
					Region = string.IsNullOrWhiteSpace(x.Region) ? "Unknown" : x.Region.Trim()
				})
				.Select(g =>
				{
					var visitCount = g.Sum(x => x.VisitCount);

					return new MunicipalityProgressResponse
					{
						Name = g.Key.Municipality,
						Region = g.Key.Region,
						VisitCount = visitCount,
						Status = ResolveStatus(visitCount),
						CompletionPercent = ResolveCompletionPercent(visitCount)
					};
				})
				.OrderBy(x => x.Name)
				.ToList();
		}
	}
}