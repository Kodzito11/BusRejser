using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

namespace BusRejserLibrary.Models
{

	public enum BusStatus
	{
		Aktiv,
		Inaktiv,
		Vedligeholdelse
	}

	public enum BusType
	{
		StorTurBus,
		MiniBus,
		VIPBus,
		Shuttle,
		Andet
	}

	public class Bus
	{
		public int busId { get; set; }
		public string Registreringnummer { get; set; }
		public string Model { get; set; }
		public string Busselskab { get; set; }
		public BusStatus Status { get; set; }
		public BusType Type { get; set; }
		public int Kapasitet { get; set; }
		public List<Facilitet> Faceliteter { get; set; } = new();
		public string? ImageUrl { get; set; }

		private Bus()
		{
			Registreringnummer = string.Empty;
			Model = string.Empty;
			Busselskab = string.Empty;
		}

		///Factory method
		private Bus
			(
			string regNr,
			string model,
			string busselskab,
			BusStatus status,
			BusType type,
			int kapasitet,
			string? imageUrl
			)
		{
			Registreringnummer = regNr;
			Model = model;
			Busselskab = busselskab;
			Status = status;
			Type = type;
			Kapasitet = kapasitet;
			ImageUrl = imageUrl;
		}

		public static Bus Create(
			string regNr,
			string model,
			string busselskab,
			BusStatus status,
			BusType type,
			int kapasitet,
			string imageUrl
			)
		{

			if (string.IsNullOrWhiteSpace(regNr))
				throw new ArgumentNullException("Registreingsnummer Kræves.");

			if (string.IsNullOrWhiteSpace(model))
				throw new ArgumentNullException("model");

			if (kapasitet <= 0)
				throw new ArgumentOutOfRangeException(nameof(kapasitet));

			return new Bus(regNr, model, busselskab, status, type, kapasitet, imageUrl);

		}

		/// Domain method

		public void AddFacilitet(Facilitet facilitet)
		{
			if (facilitet == null)
				throw new ArgumentNullException(nameof(facilitet));


			Faceliteter.Add(facilitet);
		}

		public void SetStatus(BusStatus newStatus)
		{

			Status = newStatus;
		}

	}


}