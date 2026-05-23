using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusRejserLibrary.Models
{
	public class EmailVerificationToken
	{
		[Key]
		public int EmailVerificationTokenId { get; set; }

		public int UserId { get; set; }

		[Required]
		public string TokenHash { get; set; } = "";

		public DateTime ExpiresAt { get; set; }

		public DateTime? UsedAt { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		[ForeignKey(nameof(UserId))]
		public User? User { get; set; }

		[NotMapped]
		public bool IsActive => UsedAt == null && ExpiresAt > DateTime.UtcNow;
	}
}