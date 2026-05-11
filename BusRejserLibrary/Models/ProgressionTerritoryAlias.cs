namespace BusRejserLibrary.Models
{
	public class ProgressionTerritoryAlias
	{
		public int ProgressionTerritoryAliasId { get; set; }

		public int ProgressionTerritoryId { get; set; }
		public ProgressionTerritory Territory { get; set; } = null!;

		public string Value { get; set; } = string.Empty; // danmark, denmark, dk, deutschland
	}
}