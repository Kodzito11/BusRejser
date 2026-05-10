using BusRejserLibrary.Database;
using BusRejserLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace BusRejserLibrary.Repositories
{
	public class GeoNamePlaceRepository
	{
		private readonly BusPlanenDbContext _context;

		public GeoNamePlaceRepository(BusPlanenDbContext context)
		{
			_context = context;
		}

		public List<GeoNamePlace> Search(string query, int limit = 10)
		{
			if (string.IsNullOrWhiteSpace(query))
				return new List<GeoNamePlace>();

			var normalizedQuery = NormalizeSearch(query);

			var results = _context.GeoNamePlaces
				.AsNoTracking()
				.Where(place =>
					place.Name.ToLower().Contains(normalizedQuery) ||
					(place.AsciiName != null && place.AsciiName.ToLower().Contains(normalizedQuery)) ||
					_context.GeoAlternateNames.Any(alt =>
						alt.GeoNameId == place.GeoNameId &&
						alt.AlternateName.ToLower().Contains(normalizedQuery)))
				.OrderByDescending(place =>
					place.Name.ToLower().StartsWith(normalizedQuery))
				.ThenByDescending(place =>
					place.AsciiName != null &&
					place.AsciiName.ToLower().StartsWith(normalizedQuery))
				.ThenByDescending(place =>
					_context.GeoAlternateNames.Any(alt =>
						alt.GeoNameId == place.GeoNameId &&
						alt.AlternateName.ToLower().StartsWith(normalizedQuery)))
				.ThenByDescending(place => place.Population)
				.Take(limit)
				.ToList();

			return results;
		}

		private static string NormalizeSearch(string value)
		{
			return value
				.Trim()
				.ToLower()
				.Replace("æ", "ae")
				.Replace("ø", "o")
				.Replace("å", "a");
		}
	}
}