using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusRejserLibrary.Models
{
	public class UserBagde
	{
		public int UserBadgeId { get; set; }
		public int UserBagdeId { get; set; }
		public int UserId { get; set; }
		public int BagdeId { get; set; }
		
		public DateTime DateEarned { get; set; }

	}
}
