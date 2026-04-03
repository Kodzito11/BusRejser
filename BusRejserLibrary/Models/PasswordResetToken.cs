using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusRejserLibrary.Models
{
	public class PasswordResetToken
	{
		public int Id { get; set; }
		public int UserId { get; set; }
		public string TokenHash { get; set; } = "";

		public DateTime ExpiresAt { get; set; }
		public DateTime? UsedAt { get; set; }
		public DateTime CreatedAt { get; set; }
	}
}
