using BusRejser.Features.Progression.DTOs;
using BusRejserLibrary.Models;

namespace BusRejser.Features.Progression.Services
{
	public class ProgressionMapBuilder
	{
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
			var definitions = new[]
			{
				new
				{
					Key = "dk",
					Name = "Danmark",
					Type = "country",
					Matches = new[] { "danmark", "denmark", "dk" }
				},
				new
				{
					Key = "germany",
					Name = "Tyskland",
					Type = "country",
					Matches = new[] { "tyskland", "germany", "de" }
				},
				new
				{
					Key = "czechia",
					Name = "Tjekkiet",
					Type = "country",
					Matches = new[] { "tjekkiet", "czechia", "czech republic", "cz" }
				},
				new
				{
					Key = "netherlands",
					Name = "Holland",
					Type = "country",
					Matches = new[] { "holland", "netherlands", "nl" }
				},
				new
				{
					Key = "sweden",
					Name = "Sverige",
					Type = "country",
					Matches = new[] { "sverige", "sweden", "se" }
				},
				new
				{
					Key = "norway",
					Name = "Norge",
					Type = "country",
					Matches = new[] { "norge", "norway", "no" }
				}
			};

			return definitions.Select(def =>
			{
				var visitCount = locations
					.Where(x =>
						def.Matches.Contains(Normalize(x.Country)) ||
						def.Matches.Contains(Normalize(x.Name)))
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
						Status = ResolveStatus(visitCount),
						CompletionPercent = ResolveCompletionPercent(visitCount)
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
	}
}