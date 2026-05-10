namespace BusRejser.Features.Progression.DTOs
{
	public class MunicipalityProgressResponse
	{
		public string Name { get; set; } = string.Empty;
		public string Region { get; set; } = string.Empty;
		public string Status { get; set; } = string.Empty;
		public int VisitCount { get; set; }
		public int CompletionPercent { get; set; }
	}
}
