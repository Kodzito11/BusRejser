using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusRejserLibrary.Models
{
	public class UserBadge
	{
		public int UserBadgeId { get; set; }

		public int UserId { get; set; }
		public User? User { get; set; }

		public int BadgeId { get; set; }
		public Badge? Badge { get; set; }
		
		public DateTime EarnedAt { get; set; }

	}
}
