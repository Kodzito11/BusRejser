using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusRejserLibrary.Enums;

namespace BusRejserLibrary.Models
{
	public class Badge
	{
		public int BadgeId { get; set; }
		public string BadgeName { get; set; } = null!;
		public string Description { get; set; } = null!;
		public string Country { get; set; } = string.Empty;
		public string Region { get; set; }= string.Empty;
		public string? Municipality { get; set; }
		public BadgeTier Tier { get; set; }
		public IsBadgeActive IsActive { get; set; }

	}
}
