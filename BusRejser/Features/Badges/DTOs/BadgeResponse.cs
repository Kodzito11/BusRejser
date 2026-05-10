namespace BusRejser.Features.Badges.DTOs
{
	public class BadgeResponse
	{
		public int BadgeId { get; set; }

		public string Name { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;

		public string IconUrl { get; set; } = string.Empty;

		public string Slug { get; set; } = string.Empty;
		public string Tier { get; set; } = string.Empty;
		public string? RuleValue { get; set; }
		public int? RuleWindowValue { get; set; }

		public string RuleType { get; set; } = string.Empty;
		public int RequiredValue { get; set; }
	}
}