namespace BusRejser.DTOs
{
	public class RejseResponse
	{
		public int RejseId { get; set; }
		public string Title { get; set; } = "";
		public string Destination { get; set; } = "";
		public DateTime StartAt { get; set; }
		public DateTime EndAt { get; set; }
		public decimal Price { get; set; }
		public int MaxSeats { get; set; }
		public int BookedSeats { get; set; }
		public int? BusId { get; set; }
	}
}