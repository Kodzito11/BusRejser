using BusRejser.DTOs;
using BusRejserLibrary.Models;

namespace BusRejser.Mappers
{
	public static class BusMapper
	{
		public static BusResponse ToResponse(Bus bus)
		{
			return new BusResponse
			{
				BusId = bus.busId,
				Registreringnummer = bus.Registreringnummer,
				Model = bus.Model,
				Busselskab = bus.Busselskab,
				Status = bus.Status,
				Type = bus.Type,
				Kapasitet = bus.Kapasitet,
				ImageUrl = bus.ImageUrl
			};
		}
	}
}