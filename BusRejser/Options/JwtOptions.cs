namespace BusRejser.Options
{
	public class JwtOptions
	{
		public const string SectionName = "Jwt";

		public string Secret { get; set; } = "";
		public int AccessTokenLifetimeHours { get; set; } = 12;
	}
}
