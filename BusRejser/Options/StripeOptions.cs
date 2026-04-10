namespace BusRejser.Options
{
	public class StripeOptions
	{
		public const string SectionName = "Stripe";

		public string SecretKey { get; set; } = "";
		public string WebhookSecret { get; set; } = "";
	}
}
