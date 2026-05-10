namespace BusRejser.Features.Progression.DTOs
{
	public class TerritoryProgressResponse
	{
		public string Key { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;
		public string Type { get; set; } = string.Empty;
		public string Status { get; set; } = string.Empty;
		public int VisitCount { get; set; }
		public int CompletionPercent { get; set; }
	}
}
