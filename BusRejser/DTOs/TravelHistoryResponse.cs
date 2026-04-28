namespace BusRejser.DTOs
{
	public class TravelHistoryResponse
	{
		public int TravelHistoryId { get; set; }

		public int RejseId { get; set; }
		public int BookingId { get; set; }

		public DateTime CompletedAt { get; set; }

		public string Destination { get; set; } = string.Empty;
		public string? Country { get; set; }
		public string? City { get; set; }

		public string? Region { get; set; }
		public string? Municipality { get; set; }
	}
}