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
			var normalizedQuery = NormalizeSearch(query);

			if (string.IsNullOrWhiteSpace(query))
				return new List<GeoNamePlace>();

			query = query.Trim();

			return _context.GeoNamePlaces
				.AsNoTracking()
				.Where(x =>
					x.Name.Contains(query) ||
					(x.AsciiName != null && x.AsciiName.Contains(query)))
				.OrderByDescending(x => x.Population)
				.Take(limit)
				.ToList();
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