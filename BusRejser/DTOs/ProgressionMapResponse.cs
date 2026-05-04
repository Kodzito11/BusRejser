namespace BusRejser.DTOs
{
	public class ProgressionMapResponse
	{
		public int VisitedLocationCount { get; set; }
		public int VisitedCountryCount { get; set; }
		public List<VisitedLocationMapResponse> Locations { get; set; } = new();
		public List<RegionProgressResponse> Regions { get; set; } = new();
	}

	public class VisitedLocationMapResponse
	{
		public int VisitedLocationId { get; set; }
		public string Name { get; set; } = string.Empty;
		public string Country { get; set; } = string.Empty;
		public string Region { get; set; } = string.Empty;
		public string? Municipality { get; set; }

		public double? Latitude { get; set; }
		public double? Longitude { get; set; }

		public int VisitCount { get; set; }
		public DateTime FirstVisitedAt { get; set; }
		public DateTime LastVisitedAt { get; set; }
		public bool HasCoordinates { get; set; }
	}

	public class RegionProgressResponse
	{
		public string Country { get; set; } = string.Empty;
		public string Region { get; set; } = string.Empty;
		public int VisitedLocationCount { get; set; }
		public int TotalVisitCount { get; set; }
		public DateTime LastVisitedAt { get; set; }
	}
}