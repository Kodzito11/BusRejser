namespace BusRejser.DTOs
{
	public class RejseCreateRequest
	{
		public string Title { get; set; } = "";
		public string Destination { get; set; } = "";
		public DateTime StartAt { get; set; }
		public DateTime EndAt { get; set; }
		public decimal Price { get; set; }
		public int MaxSeats { get; set; }
		public int? BusId { get; set; }
	}
}