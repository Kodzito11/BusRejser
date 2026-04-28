namespace BusRejser.DTOs
{
	public class BadgeResponse
	{
		public int BadgeId { get; set; }

		public string Name { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;

		public string IconUrl { get; set; } = string.Empty;

		public string RuleType { get; set; } = string.Empty;
		public int RequiredValue { get; set; }
	}
}