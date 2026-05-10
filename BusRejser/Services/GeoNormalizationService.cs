using BusRejser.DTOs;
using BusRejser.Services.Interfaces;
using BusRejser.Mappers;
using BusRejserLibrary.Database;
using Microsoft.EntityFrameworkCore;
using BusRejserLibrary.Models;

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
				(x.AsciiName != null && x.AsciiName.ToLower() == normalized));

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

			var municipality = geoPlace.Name ?? string.Empty;

			var region =
				DenmarkRegionMapper.GetRegionForMunicipality(municipality)
				?? geoPlace.Admin1Code
				?? string.Empty;

			return new NormalizedGeoResult
			{
				Country = geoPlace.CountryCode == "DK" ? "Denmark" : geoPlace.CountryCode,
				Region = region,
				Municipality = municipality,
				Latitude = geoPlace.Latitude,
				Longitude = geoPlace.Longitude,
			};
		}
	}
}