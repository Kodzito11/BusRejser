using BusRejser.DTOs;
using BusRejser.Services.Interfaces;
using BusRejserLibrary.Data;
using BusRejserLibrary.Database;
using Microsoft.EntityFrameworkCore;

namespace BusRejser.Services
{
	public class GeoNormalizationService : IGeoNormalizationService
	{
		private readonly BusPlanenDbContext _context;

		public GeoNormalizationService(
			BusPlanenDbContext context)
		{
			_context = context;
		}

		public NormalizedGeoResult Normalize(string destination)
		{
			if (string.IsNullOrWhiteSpace(destination))
			{
				return new NormalizedGeoResult();
			}

			var normalized = destination
				.Trim()
				.ToLowerInvariant();

			var geoPlace = _context.GeoNamePlaces
				.AsNoTracking()
				.FirstOrDefault(x =>
					x.Name.ToLower() == normalized ||
					x.AsciiName.ToLower() == normalized);

			if (geoPlace == null)
			{
				var alternate = _context.GeoAlternateNames
					.AsNoTracking()
					.FirstOrDefault(x =>
						x.AlternateName.ToLower() == normalized);

				if (alternate != null)
				{
					geoPlace = _context.GeoNamePlaces
						.AsNoTracking()
						.FirstOrDefault(x =>
							x.GeoNameId == alternate.GeoNameId);
				}
			}

			if (geoPlace == null)
			{
				return new NormalizedGeoResult
				{
					Municipality = destination
				};
			}

			return new NormalizedGeoResult
			{
				Country = geoPlace.Country ?? string.Empty,

				Region = geoPlace.Admin1Name ?? string.Empty,

				Municipality = geoPlace.Name ?? string.Empty,

				Latitude = geoPlace.Latitude,

				Longitude = geoPlace.Longitude,
			};
		}
	}
}