namespace BusRejser.Features.Badges.DTOs
{
	public class UserBadgeResponse
	{
		public int BadgeId { get; set; }

		public string Name { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;

		public string IconUrl { get; set; } = string.Empty;
		public string Slug { get; set; } = string.Empty;
		public string Tier { get; set; } = string.Empty;

		public DateTime EarnedAt { get; set; }
	}
}