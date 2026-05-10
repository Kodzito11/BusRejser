namespace BusRejser.Features.Geo.DTOs
{
	public class GeoNamePlaceResponse
	{
		public int GeoNameId { get; set; }
		public string Name { get; set; } = string.Empty;
		public string? AsciiName { get; set; }
		public string CountryCode { get; set; } = string.Empty;
		public string? Admin1Code { get; set; }
		public double? Latitude { get; set; }
		public double? Longitude { get; set; }
		public long Population { get; set; }
		public string? FeatureClass { get; set; }
		public string? FeatureCode { get; set; }
	}
}