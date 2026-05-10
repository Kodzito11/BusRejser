using BusRejser.Features.Geo.DTOs;

namespace BusRejser.Features.Geo.Services.Interfaces
{
	public interface IGeoNormalizationService
	{
		NormalizedGeoResult Normalize(string destination);
	}
}