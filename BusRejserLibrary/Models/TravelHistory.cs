using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusRejserLibrary.Models
{
	public class TravelHistory
	{
		public int TravelHistoryId { get; set; }

		public int UserId { get; set; }
		public User? User { get; set; }

		public int RejseId { get; set; }
		public Rejse? Rejse { get; set; }

		public int BookingId { get; set; }
		public Booking? Booking { get; set; }


		public string Destination { get; set; } = string.Empty;

		public string Country { get; set; } = string.Empty;
		public string City { get; set; } = string.Empty;
		public string Region { get; set; } = string.Empty;
		public string? Municipality { get; set; }

		public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
	}
}
