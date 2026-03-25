using BusRejser.DTOs;
using BusRejserLibrary.Models;

namespace BusRejser.Mappers
{
	public static class FacilitetMapper
	{
		public static FacilitetResponse ToResponse(Facilitet f)
		{
			return new FacilitetResponse
			{
				Id = f.Id,
				Name = f.Name,
				Description = f.Description,
				ExtraPrice = f.ExtraPrice,
				IsActive = f.IsActive,
				Type = f.Type
			};
		}
	}
}