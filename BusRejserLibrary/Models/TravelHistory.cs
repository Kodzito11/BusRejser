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
		public int RejseId { get; set; }
		public int BookingId { get; set; }

		public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

		public string Destination { get; set; } = string.Empty;
		public string? Country { get; set; }
		public string? City { get; set; }
		public string? Region { get; set; }
		public string? Municipality { get; set; }

	}
}
