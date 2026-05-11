namespace BusRejser.Features.Progression.DTOs
{
	public class ProgressionTerritoryAdminResponse
	{
		public int ProgressionTerritoryId { get; set; }
		public string Key { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;
		public string Type { get; set; } = string.Empty;

		public bool IsActive { get; set; }
		public bool IsVisible { get; set; }
		public bool IsComingSoon { get; set; }

		public int MasteryTarget { get; set; }
		public string? Description { get; set; }

		public List<ProgressionTerritoryAliasResponse> Aliases { get; set; } = new();
	}

	public class ProgressionTerritoryAliasResponse
	{
		public int ProgressionTerritoryAliasId { get; set; }
		public string Value { get; set; } = string.Empty;
	}

	public class CreateProgressionTerritoryRequest
	{
		public string Key { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;
		public string Type { get; set; } = "country";

		public bool IsActive { get; set; } = true;
		public bool IsVisible { get; set; } = true;
		public bool IsComingSoon { get; set; } = false;

		public int MasteryTarget { get; set; } = 10;
		public string? Description { get; set; }

		public List<string> Aliases { get; set; } = new();
	}

	public class UpdateProgressionTerritoryRequest
	{
		public string Key { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;
		public string Type { get; set; } = "country";

		public bool IsActive { get; set; }
		public bool IsVisible { get; set; }
		public bool IsComingSoon { get; set; }

		public int MasteryTarget { get; set; }
		public string? Description { get; set; }
	}

	public class AddProgressionTerritoryAliasRequest
	{
		public string Value { get; set; } = string.Empty;
	}
}