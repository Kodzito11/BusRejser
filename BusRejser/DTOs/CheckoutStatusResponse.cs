namespace BusRejser.DTOs
{
	public class CheckoutStatusResponse
	{
		public string SessionId { get; set; } = "";
		public bool IsPaid { get; set; }
		public bool BookingExists { get; set; }
		public string Status { get; set; } = "";
		public int? BookingId { get; set; }
		public string? BookingReference { get; set; }
	}
}