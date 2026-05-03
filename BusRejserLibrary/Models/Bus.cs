using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using BusRejserLibrary.Enums;

namespace BusRejserLibrary.Models
{

	public class Bus
	{
		public int BusId { get; set; }
		public string Registreringsnummer { get; set; }
		public string Model { get; set; }
		public string Busselskab { get; set; }
		public BusStatus Status { get; set; }
		public BusType Type { get; set; }
		public int Kapacitet { get; set; }
		public List<Facilitet> Faciliteter { get; set; } = new();
		public string? ImageUrl { get; set; }

		private Bus()
		{
			Registreringsnummer = string.Empty;
			Model = string.Empty;
			Busselskab = string.Empty;
		}
		private Bus
			(
			string regNr,
			string model,
			string busselskab,
			BusStatus status,
			BusType type,
			int kapacitet,
			string? imageUrl
			)
		{
			Registreringsnummer = regNr;
			Model = model;
			Busselskab = busselskab;
			Status = status;
			Type = type;
			Kapacitet = kapacitet;
			ImageUrl = imageUrl;
		}

		public static Bus Create(
			string regNr,
			string model,
			string busselskab,
			BusStatus status,
			BusType type,
			int kapacitet,
			string imageUrl
			)
		{

			if (string.IsNullOrWhiteSpace(regNr))
				throw new ArgumentNullException("Registreingsnummer Kræves.");

			if (string.IsNullOrWhiteSpace(model))
				throw new ArgumentNullException(nameof(model));

			if (kapacitet <= 0)
				throw new ArgumentOutOfRangeException(nameof(kapacitet));

			return new Bus(regNr, model, busselskab, status, type, kapacitet, imageUrl);

		}

		public void AddFacilitet(Facilitet facilitet)
		{
			if (facilitet == null)
				throw new ArgumentNullException(nameof(facilitet));


			Faciliteter.Add(facilitet);
		}

		public void SetStatus(BusStatus newStatus)
		{

			Status = newStatus;
		}

	}


}