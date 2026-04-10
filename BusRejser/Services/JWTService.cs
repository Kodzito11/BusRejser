using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BusRejser.Options;
using BusRejserLibrary.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BusRejser.Services
{
	public class JwtService
	{
		private readonly JwtOptions _jwtOptions;

		public JwtService(IOptions<JwtOptions> jwtOptions)
		{
			_jwtOptions = jwtOptions.Value;
		}

		public string GenerateToken(User user)
		{
			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Secret));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var claims = new[]
			{
				new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
				new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
				new Claim(ClaimTypes.Email, user.Email),
				new Claim(ClaimTypes.Role, user.Role.ToString()),
				new Claim("username", user.Username)
			};

			var token = new JwtSecurityToken(
				issuer: _jwtOptions.Issuer,
				audience: _jwtOptions.Audience,
				claims: claims,
				notBefore: DateTime.UtcNow,
				expires: GetAccessTokenExpiresAtUtc(),
				signingCredentials: creds
			);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}

		public DateTime GetAccessTokenExpiresAtUtc()
		{
			return DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenLifetimeMinutes);
		}

		public string GenerateRefreshToken()
		{
			return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
		}
	}
}
