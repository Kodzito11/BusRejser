namespace BusRejser.DTOs
{
	public class AuthTokenResponse
	{
		public string TokenType { get; set; } = "Bearer";
		public string AccessToken { get; set; } = "";
		public DateTime AccessTokenExpiresAt { get; set; }
		public string RefreshToken { get; set; } = "";
		public DateTime RefreshTokenExpiresAt { get; set; }
		public AuthenticatedUserResponse User { get; set; } = new();
	}
}
