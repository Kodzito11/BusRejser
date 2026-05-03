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
				BusId = bus.BusId,
				Registreringnummer = bus.Registreringsnummer,
				Model = bus.Model,
				Busselskab = bus.Busselskab,
				Status = bus.Status,
				Type = bus.Type,
				Kapasitet = bus.Kapacitet,
				ImageUrl = bus.ImageUrl
			};
		}
	}
}