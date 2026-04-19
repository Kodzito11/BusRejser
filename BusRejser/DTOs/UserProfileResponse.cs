namespace BusRejser.DTOs
{
	public class UserProfileResponse
	{
		public int UserId { get; set; }
		public string FirstName { get; set; } = string.Empty;
		public string LastName { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string Role { get; set; } = string.Empty;
		public DateTime CreatedAt { get; set; }
		public string? Phone { get; set; }
	}
}
