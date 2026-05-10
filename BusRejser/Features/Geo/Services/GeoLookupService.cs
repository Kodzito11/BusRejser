using BusRejser.Features.Geo.DTOs;
using BusRejserLibrary.Repositories;

namespace BusRejser.Features.Geo.Services
{
	public class GeoLookupService
	{
		private readonly GeoNamePlaceRepository _geoNamePlaceRepository;

		public GeoLookupService(GeoNamePlaceRepository geoNamePlaceRepository)
		{
			_geoNamePlaceRepository = geoNamePlaceRepository;
		}

		public List<GeoNamePlaceResponse> Search(string query)
		{
			return _geoNamePlaceRepository.Search(query)
				.Select(x => new GeoNamePlaceResponse
				{
					GeoNameId = x.GeoNameId,
					Name = x.Name,
					AsciiName = x.AsciiName,
					CountryCode = x.CountryCode,
					Admin1Code = x.Admin1Code,
					Latitude = x.Latitude,
					Longitude = x.Longitude,
					Population = x.Population,
					FeatureClass = x.FeatureClass,
					FeatureCode = x.FeatureCode
				})
				.ToList();
		}
	}
}