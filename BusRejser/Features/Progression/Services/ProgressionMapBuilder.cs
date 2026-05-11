using BusRejser.Features.Progression.DTOs;
using BusRejserLibrary.Models;
using BusRejserLibrary.Repositories;

namespace BusRejser.Features.Progression.Services
{
	public class ProgressionMapBuilder
	{
		private readonly ProgressionTerritoryRepository _territoryRepository;

		public ProgressionMapBuilder(ProgressionTerritoryRepository territoryRepository)
		{
			_territoryRepository = territoryRepository;
		}

		public ProgressionMapResponse Build(List<VisitedLocation> locations)
		{
			return new ProgressionMapResponse
			{
				VisitedLocationCount = locations.Count,

				VisitedCountryCount = locations
					.Where(x => !string.IsNullOrWhiteSpace(x.Country))
					.Select(x => x.Country.Trim().ToLowerInvariant())
					.Distinct()
					.Count(),

				Locations = BuildLocations(locations),
				Regions = BuildRegions(locations),
				Territories = BuildTerritories(locations),
				Municipalities = BuildMunicipalities(locations)
			};
		}

		public List<TerritoryProgressResponse> BuildTerritories(List<VisitedLocation> locations)
		{
			var territories = _territoryRepository.GetVisibleWithAliases();

			return territories.Select(territory =>
			{
				var aliases = territory.Aliases
					.Select(x => Normalize(x.Value))
					.Append(Normalize(territory.Key))
					.Append(Normalize(territory.Name))
					.Distinct()
					.ToList();

				var visitCount = territory.IsActive
					? locations
						.Where(x =>
							aliases.Contains(Normalize(x.Country)) ||
							aliases.Contains(Normalize(x.Name)))
						.Sum(x => x.VisitCount)
					: 0;

				return new TerritoryProgressResponse
				{
					Key = territory.Key,
					Name = territory.Name,
					Type = territory.Type,
					VisitCount = visitCount,
					Status = ResolveTerritoryStatus(visitCount, territory),
					CompletionPercent = ResolveTerritoryCompletionPercent(
						visitCount,
						territory.MasteryTarget
					)
				};
			}).ToList();
		}

		public List<MunicipalityProgressResponse> BuildMunicipalities(List<VisitedLocation> locations)
		{
			return locations
				.Where(x => !string.IsNullOrWhiteSpace(x.Municipality))
				.GroupBy(x => new
				{
					Municipality = x.Municipality!.Trim(),
					Region = string.IsNullOrWhiteSpace(x.Region)
						? "Unknown"
						: x.Region.Trim()
				})
				.Select(g =>
				{
					var visitCount = g.Sum(x => x.VisitCount);

					return new MunicipalityProgressResponse
					{
						Name = g.Key.Municipality,
						Region = g.Key.Region,
						VisitCount = visitCount,
						Status = ResolveLocationStatus(visitCount),
						CompletionPercent = ResolveLocationCompletionPercent(visitCount)
					};
				})
				.OrderBy(x => x.Name)
				.ToList();
		}

		private static List<VisitedLocationMapResponse> BuildLocations(List<VisitedLocation> locations)
		{
			return locations.Select(x => new VisitedLocationMapResponse
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
			}).ToList();
		}

		private static List<RegionProgressResponse> BuildRegions(List<VisitedLocation> locations)
		{
			return locations
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
				.ToList();
		}

		private static string Normalize(string? value)
		{
			return (value ?? "")
				.Trim()
				.ToLowerInvariant();
		}

		private static string ResolveTerritoryStatus(
			int visitCount,
			ProgressionTerritory territory)
		{
			if (!territory.IsActive || territory.IsComingSoon)
				return "locked";

			if (visitCount >= territory.MasteryTarget)
				return "mastered";

			if (visitCount >= 1)
				return "unlocked";

			return "locked";
		}

		private static int ResolveTerritoryCompletionPercent(
			int visitCount,
			int masteryTarget)
		{
			if (visitCount <= 0) return 0;
			if (masteryTarget <= 0) return 0;
			if (visitCount >= masteryTarget) return 100;

			return (int)Math.Round((double)visitCount / masteryTarget * 100);
		}

		private static string ResolveLocationStatus(int visitCount)
		{
			if (visitCount >= 10) return "mastered";
			if (visitCount >= 1) return "unlocked";

			return "locked";
		}

		private static int ResolveLocationCompletionPercent(int visitCount)
		{
			if (visitCount <= 0) return 0;
			if (visitCount >= 10) return 100;

			return visitCount * 10;
		}
	}
}