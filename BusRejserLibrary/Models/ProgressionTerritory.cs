namespace BusRejserLibrary.Models
{
	public class ProgressionTerritory
	{
		public int ProgressionTerritoryId { get; set; }

		public string Key { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;
		public string Type { get; set; } = "country";

		public bool IsActive { get; set; } = true;
		public bool IsVisible { get; set; } = true;
		public bool IsComingSoon { get; set; } = false;

		public int MasteryTarget { get; set; } = 10;

		public string? Description { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime? UpdatedAt { get; set; }

		public List<ProgressionTerritoryAlias> Aliases { get; set; } = new();
	}
}