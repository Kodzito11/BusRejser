namespace BusRejser.DTOs
{
	public class AuthenticatedUserResponse
	{
		public int UserId { get; set; }
		public string Username { get; set; } = "";
		public string Email { get; set; } = "";
		public string Role { get; set; } = "";
	}
}
