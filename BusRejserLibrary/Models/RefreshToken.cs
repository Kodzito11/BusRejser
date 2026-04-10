namespace BusRejserLibrary.Models
{
	public class RefreshToken
	{
		public int Id { get; set; }
		public int UserId { get; set; }
		public string TokenHash { get; set; } = string.Empty;
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime ExpiresAt { get; set; }
		public DateTime? LastUsedAt { get; private set; }
		public DateTime? RevokedAt { get; private set; }
		public string? ReplacedByTokenHash { get; private set; }

		public bool IsActive => RevokedAt == null && ExpiresAt > DateTime.UtcNow;

		public void MarkUsed()
		{
			LastUsedAt = DateTime.UtcNow;
		}

		public void Revoke(string? replacedByTokenHash = null)
		{
			if (RevokedAt != null)
				return;

			RevokedAt = DateTime.UtcNow;
			ReplacedByTokenHash = replacedByTokenHash;
		}
	}
}
