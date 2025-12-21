using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusRejserLibrary.Models
{
	public enum FacilitetType
	{
		Komfort,
		Sikkerhed,
		Underholdning,
		Andet
	}

	public class Facilitet
	{
		public int Id { get; private set; }
		public string Name { get; private set; }
		public string Description { get; private set; }
		public decimal ExtraPrice { get; private set; }
		public bool IsActive { get; private set; }
		public FacilitetType Type { get; private set; }

		private Facilitet(string name, string description, decimal extraPrice, FacilitetType type, bool isActive)
		{
			Name = name;
			Description = description;
			ExtraPrice = extraPrice;
			Type = type;
			IsActive = isActive;

		}

		public static Facilitet Create(string name, string description, decimal extraPrice, FacilitetType type, bool isActive)
		{
			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentException("Navn på facilitet kræves.");

			if (extraPrice < 0) throw new ArgumentOutOfRangeException(nameof(extraPrice), "Extra pris kan ikke være negativ");

			return new Facilitet(name, description, extraPrice, type, isActive);
		}

		public void UpdatePrice(decimal newPrice)
		{
			if (newPrice < 0) throw new ArgumentOutOfRangeException(nameof(newPrice), "Pris kan ikke være negativ.");

			ExtraPrice = newPrice;
		}

		public void Deactivate() => IsActive = false;
	}
}