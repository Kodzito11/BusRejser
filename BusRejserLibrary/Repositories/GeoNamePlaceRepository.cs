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

			query = query.Trim().ToLower();

			var results = _context.GeoNamePlaces
				.AsNoTracking()
				.Where(place =>
					place.Name.ToLower().Contains(query) ||
					(place.AsciiName != null && place.AsciiName.ToLower().Contains(query)) ||
					_context.GeoAlternateNames.Any(alt =>
						alt.GeoNameId == place.GeoNameId &&
						alt.AlternateName.ToLower().Contains(query)))
				.OrderByDescending(place =>
					place.Name.ToLower().StartsWith(query) ||
					(place.AsciiName != null && place.AsciiName.ToLower().StartsWith(query)))
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