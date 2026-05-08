namespace BusRejserLibrary.Models;

public class GeoAlternateName
{
	public int GeoAlternateNameId { get; set; }

	public int GeoNameId { get; set; }

	public string AlternateName { get; set; } = string.Empty;

	public string? IsoLanguage { get; set; }

	public bool IsPreferredName { get; set; }

	public bool IsShortName { get; set; }

	public virtual GeoNamePlace? GeoNamePlace { get; set; }
}