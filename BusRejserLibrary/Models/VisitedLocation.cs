using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusRejserLibrary.Models
{
	public class VisitedLocation
	{
		public int VisitedLocationId { get; set; }

		public int UserId { get; set; }

		public string Name { get; set; } = string.Empty;
		public string Country { get; set; } = string.Empty; 
		public string Region { get; set; } = string.Empty;
		public string? Municipality { get; set; }

		public double? Latitude { get; set; }
		public double? Longitude { get; set; }

		public DateTime FirstVisitedAt { get; set; } = DateTime.UtcNow;
		public DateTime LastVisitedAt { get; set; } = DateTime.UtcNow;

		public int VisitCount { get; set; } = 1;

	}
}
