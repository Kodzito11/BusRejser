namespace BusRejser.Options
{
	public class EmailOptions
	{
		public const string SectionName = "Email";

		public string Host { get; set; } = "";
		public int Port { get; set; }
		public string Username { get; set; } = "";
		public string Password { get; set; } = "";
		public string From { get; set; } = "";
	}
}
