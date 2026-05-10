using BusRejser.DTOs;

namespace BusRejser.Services.Interfaces
{
	public interface IGeoNormalizationService
	{
		NormalizedGeoResult Normalize(string destination);
	}
}