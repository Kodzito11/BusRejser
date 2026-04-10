namespace BusRejser.Options
{
	public class FrontendOptions
	{
		public const string SectionName = "Frontend";

		public string BaseUrl { get; set; } = "";
		public string PaymentSuccessPath { get; set; } = "/betaling/success";
		public string PaymentCancelPath { get; set; } = "/betaling/cancel";
		public string PasswordResetPath { get; set; } = "/reset-password";
	}
}
