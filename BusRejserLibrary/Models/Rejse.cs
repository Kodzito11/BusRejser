using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusRejserLibrary.Models
{
	public class Rejse
	{
		public int RejseId { get; set; }
		public string Title { get; set; }
		public string Destination { get; set; }
		public DateTime StartAt { get; set; }
		public DateTime EndAt { get; set; }
		public decimal Price { get; set; }
		public int MaxSeats { get; set; }
		public int? BusId { get; set; }
		public int BookedSeats { get; set; }

		private Rejse(string title, string destination, DateTime startAt, DateTime endAt, decimal price, int maxSeats, int? busId)
		{
			Title = title;
			Destination = destination;
			StartAt = startAt;
			EndAt = endAt;
			Price = price;
			MaxSeats = maxSeats;
			BusId = busId;
			BookedSeats = 0;
		}

		public static Rejse Create(string title, string destination, DateTime startAt, DateTime endAt, decimal price, int maxSeats, int? busId)
		{
			if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title kræves.");
			if (string.IsNullOrWhiteSpace(destination)) throw new ArgumentException("Destination kræves.");
			if (endAt < startAt) throw new ArgumentException("EndAt kan ikke være før StartAt.");
			if (price < 0) throw new ArgumentOutOfRangeException(nameof(price));
			if (maxSeats < 0) throw new ArgumentOutOfRangeException(nameof(maxSeats));

			return new Rejse(title, destination, startAt, endAt, price, maxSeats, busId);
		}
	}
}
