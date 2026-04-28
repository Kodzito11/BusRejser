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
		public string? Municipality { get; set; } //JSON eller string

		public string IconUrl { get; set; } = string.Empty;

		public string RuleType { get; set; } = string.Empty;
		public string? RuleValue { get; set; }
		public int RequiredValue { get; set; }
		public int? RuleWindowValue { get; set; }

		public List<UserBadge> UserBadges { get; set; } = new();

		public BadgeTier Tier { get; set; }
		public IsBadgeActive IsActive { get; set; }

	}
}
