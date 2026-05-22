namespace BusRejser.Features.Auth.DTOs
{
	public class AuthSessionResponse
	{
		public string TokenType { get; set; } = "Bearer";
		public string AccessToken { get; set; } = "";
		public DateTime AccessTokenExpiresAt { get; set; }
		public DateTime RefreshTokenExpiresAt { get; set; }
		public AuthenticatedUserResponse User { get; set; } = new();
	}
}