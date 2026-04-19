using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusRejserLibrary.Enums;

namespace BusRejserLibrary.Models
{
	public class User
	{
		public int UserId { get; set; }

		public string FirstName { get; set; } = string.Empty;
		public string LastName { get; set; } = string.Empty;

		[NotMapped]
		public string FullName => $"{FirstName} {LastName}".Trim();

		public string Email { get; set; } = string.Empty;
		public string? PhoneNumber { get; set; }


		public string PasswordHash { get; set; } = string.Empty;

		public bool IsActive { get; set; } = true;
		public bool EmailConfirmed { get; set; } = false;

		public UserRole Role { get; set; } = Enums.UserRole.Kunde;

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime? UpdatedAt { get; set; }
		public DateTime? LastLoginAt {get; set;}

	}
}
