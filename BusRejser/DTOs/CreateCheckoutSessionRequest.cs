namespace BusRejser.DTOs
{
	public class CreateCheckoutSessionRequest
	{
		public int RejseId { get; set; }
		public int AntalPladser { get; set; }
		public string KundeNavn { get; set; } = "";
		public string KundeEmail { get; set; } = "";
	}
}