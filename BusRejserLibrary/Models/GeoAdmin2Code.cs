namespace BusRejserLibrary.Models
{
	public class GeoAdmin2Code
	{
		public int Id { get; set; }

		public string Code { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;
		public string? AsciiName { get; set; }
		public int? GeoNameId { get; set; }
	}
}