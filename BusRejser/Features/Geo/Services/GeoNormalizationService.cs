using BusRejser.Mappers;
using BusRejserLibrary.Database;
using Microsoft.EntityFrameworkCore;
using BusRejserLibrary.Models;
using BusRejser.Features.Geo.DTOs;
using BusRejser.Features.Geo.Services.Interfaces;

namespace BusRejser.Features.Geo.Services
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
				x.AsciiName != null && x.AsciiName.ToLower() == normalized);

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

			if (geoPlace.CountryCode == "DK")
			{
				region = geoPlace.Admin1Code switch
				{
					"17" => "Region Hovedstaden",
					"18" => "Region Midtjylland",
					"19" => "Region Nordjylland",
					"20" => "Region Sjælland",
					"21" => "Region Syddanmark",
					_ => region
				};

				if (!string.IsNullOrWhiteSpace(geoPlace.Admin1Code) &&
					!string.IsNullOrWhiteSpace(geoPlace.Admin2Code))
				{
					var admin2Key = $"DK.{geoPlace.Admin1Code}.{geoPlace.Admin2Code}";

					var admin2 = _context.GeoAdmin2Codes
						.AsNoTracking()
						.FirstOrDefault(x => x.Code == admin2Key);

					if (admin2 != null)
					{
						municipality = admin2.Name
							.Replace(" Kommune", "")
							.Trim();
					}
				}
			}

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