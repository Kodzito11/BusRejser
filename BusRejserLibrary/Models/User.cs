using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusRejserLibrary.Enums;

namespace BusRejserLibrary.Models
{
	public class User
	{
		public int Id { get; set; }

		public string Username { get; set; } = string.Empty;

		public string Email { get; set; } = string.Empty;

		public string PasswordHash { get; set; } = string.Empty;

		public Enums.UserRole Role { get; set; } = Enums.UserRole.Kunde;

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		public string? FullName { get; set; }

		public string? Phone { get; set; }
	}

}
