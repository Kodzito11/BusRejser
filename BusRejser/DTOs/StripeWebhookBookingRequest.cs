namespace BusRejser.DTOs
{
	public class StripeWebhookBookingRequest
	{
		public int RejseId { get; set; }
		public int AntalPladser { get; set; }
		public string KundeNavn { get; set; } = "";
		public string KundeEmail { get; set; } = "";
		public int? UserId { get; set; }

		public string StripeSessionId { get; set; } = "";
		public string? StripePaymentIntentId { get; set; }

		public decimal TotalPrice { get; set; }
	}
}