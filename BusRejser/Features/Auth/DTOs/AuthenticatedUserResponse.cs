namespace BusRejser.Features.Auth.DTOs
{
	public class AuthenticatedUserResponse
	{
		public int UserId { get; set; }
		public string FirstName { get; set; } = "";
		public string LastName { get; set; } = "";
		public string FullName { get; set; } = "";
		public string Email { get; set; } = "";
		public string Role { get; set; } = "";
	}
}
