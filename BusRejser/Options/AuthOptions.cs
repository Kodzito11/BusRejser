namespace BusRejser.Options
{
	public class AuthOptions
	{
		public const string SectionName = "Auth";

		public int RefreshTokenLifetimeDays { get; set; } = 14;
		public bool RequireConfirmedEmail { get; set; }
	}
}
