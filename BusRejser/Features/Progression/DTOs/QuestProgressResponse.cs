namespace BusRejser.Features.Progression.DTOs
{
	public class QuestProgressResponse
	{
		public string Key { get; set; } = string.Empty;
		public string Title { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public string Status { get; set; } = string.Empty;
		public int Current { get; set; }
		public int Target { get; set; }
		public int CompletionPercent { get; set; }
		public string RewardLabel { get; set; } = string.Empty;
	}
}