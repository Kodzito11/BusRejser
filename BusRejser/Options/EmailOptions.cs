namespace BusRejser.Options
{
	public class EmailOptions
	{
		public const string SectionName = "Email";

		public string Provider { get; set; } = "Resend";
		public string From { get; set; } = "";
		public string ApiKey { get; set; } = "";
	}
}