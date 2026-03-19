using System;
using BusRejserLibrary.Enums;

namespace BusRejser.DTOs
{
	public class BookingResponse
	{
		public int BookingId { get; set; }
		public int RejseId { get; set; }
		public int? UserId { get; set; }

		public string? Role { get; set; }

		public string KundeNavn { get; set; } = "";
		public string KundeEmail { get; set; } = "";

		public int AntalPladser { get; set; }
		public DateTime CreatedAt { get; set; }

		public int Status { get; set; }

		public string BookingReference { get; set; } = "";
	}
}