namespace BusRejser.DTOs
{
	public class NormalizedGeoResult
	{
		public string Country { get; set; } = string.Empty;

		public string Region { get; set; } = string.Empty;

		public string Municipality { get; set; } = string.Empty;

		public double? Latitude { get; set; }

		public double? Longitude { get; set; }
	}
}