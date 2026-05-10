using BusRejser.Features.Geo.DTOs;

namespace BusRejser.Services.Interfaces
{
	public interface IGeoNormalizationService
	{
		NormalizedGeoResult Normalize(string destination);
	}
}