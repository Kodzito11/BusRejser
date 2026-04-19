using BusRejser.DTOs;
using BusRejser.Exceptions;
using BusRejser.Options;
using BusRejserLibrary.Models;
using BusRejserLibrary.Repositories;
using Microsoft.Extensions.Options;

namespace BusRejser.Services
{
	public class AuthService
	{
		private readonly UserRepository _userRepository;
		private readonly PasswordService _passwordService;
		private readonly JwtService _jwtService;
		private readonly PasswordResetTokenRepository _passwordResetTokenRepository;
		private readonly RefreshTokenRepository _refreshTokenRepository;
		private readonly EmailService _emailService;
		private readonly FrontendOptions _frontendOptions;
		private readonly AuthOptions _authOptions;

		public AuthService(
			UserRepository userRepository,
			PasswordService passwordService,
			JwtService jwtService,
			PasswordResetTokenRepository passwordResetTokenRepository,
			RefreshTokenRepository refreshTokenRepository,
			EmailService emailService,
			IOptions<FrontendOptions> frontendOptions,
			IOptions<AuthOptions> authOptions)
		{
			_userRepository = userRepository;
			_passwordService = passwordService;
			_jwtService = jwtService;
			_passwordResetTokenRepository = passwordResetTokenRepository;
			_refreshTokenRepository = refreshTokenRepository;
			_emailService = emailService;
			_frontendOptions = frontendOptions.Value;
			_authOptions = authOptions.Value;
		}

		public int Register(string FirstName, string LastName, string email, string password)
		{
			if (string.IsNullOrWhiteSpace(FirstName))
				throw new ValidationException("Fornavn kræves");
			if (string.IsNullOrWhiteSpace(LastName))
				throw new ValidationException("Efternavn kræves");
			if (string.IsNullOrWhiteSpace(email))
				throw new ValidationException("Email kræves.");

			if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
				throw new ValidationException("Password skal være mindst 8 tegn.");

			email = email.Trim().ToLowerInvariant();

			var existingEmail = _userRepository.GetByEmail(email);
			if (existingEmail != null)
				throw new ConflictException("Email findes allerede.");

			var passwordHash = _passwordService.HashPassword(password);

			var user = new User
			{	
				FirstName = FirstName.Trim(),
				LastName = LastName.Trim(),
				Email = email,
				PasswordHash = passwordHash,
				Role = BusRejserLibrary.Enums.UserRole.Kunde,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			};

			return _userRepository.Create(user);
		}

		public AuthTokenResponse Login(string email, string password)
		{
			if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
				throw new UnauthorizedException("Forkert email eller password.");

			email = email.Trim().ToLowerInvariant();

			var user = _userRepository.GetByEmail(email);
			if (user == null)
				throw new UnauthorizedException("Forkert email eller password.");

			var isValid = _passwordService.VerifyPassword(password, user.PasswordHash);
			if (!isValid)
				throw new UnauthorizedException("Forkert email eller password.");

			EnsureUserCanAuthenticate(user);

			user.LastLoginAt = DateTime.UtcNow;
			var updated = _userRepository.Update(user);
			if (!updated)
				throw new ConflictException("Bruger kunne ikke opdateres.");

			return IssueSession(user);
		}

		public AuthTokenResponse Refresh(string refreshToken)
		{
			if (string.IsNullOrWhiteSpace(refreshToken))
				throw new UnauthorizedException("Refresh token mangler.");

			var refreshTokenHash = Security.TokenHasher.Hash(refreshToken);
			var existingToken = _refreshTokenRepository.GetByTokenHash(refreshTokenHash);

			if (existingToken == null || !existingToken.IsActive)
				throw new UnauthorizedException("Refresh token er ugyldig eller udløbet.");

			var user = _userRepository.GetById(existingToken.UserId);
			if (user == null)
				throw new UnauthorizedException("Sessionen er ikke længere gyldig.");

			EnsureUserCanAuthenticate(user);

			var rawNewRefreshToken = _jwtService.GenerateRefreshToken();
			var newRefreshTokenHash = Security.TokenHasher.Hash(rawNewRefreshToken);
			var newRefreshToken = new RefreshToken
			{
				UserId = user.UserId,
				TokenHash = newRefreshTokenHash,
				CreatedAt = DateTime.UtcNow,
				ExpiresAt = DateTime.UtcNow.AddDays(_authOptions.RefreshTokenLifetimeDays)
			};

			_refreshTokenRepository.Rotate(existingToken, newRefreshToken);

			return BuildAuthTokenResponse(user, rawNewRefreshToken, newRefreshToken.ExpiresAt);
		}

		public void Logout(string refreshToken)
		{
			if (string.IsNullOrWhiteSpace(refreshToken))
				return;

			var refreshTokenHash = Security.TokenHasher.Hash(refreshToken);
			var existingToken = _refreshTokenRepository.GetByTokenHash(refreshTokenHash);
			if (existingToken == null || existingToken.RevokedAt != null)
				return;

			_refreshTokenRepository.Revoke(existingToken);
		}

		public async Task ForgotPassword(string email)
		{
			if (string.IsNullOrWhiteSpace(email))
				return;

			var normalizedEmail = email.Trim().ToLowerInvariant();
			var user = _userRepository.GetByEmail(normalizedEmail);

			if (user == null)
				return;

			_passwordResetTokenRepository.InvalidateAllForUser(user.UserId);

			var rawToken = Guid.NewGuid().ToString();
			var tokenHash = Security.TokenHasher.Hash(rawToken);

			var token = new PasswordResetToken
			{
				UserId = user.UserId,
				TokenHash = tokenHash,
				ExpiresAt = DateTime.UtcNow.AddMinutes(30),
				CreatedAt = DateTime.UtcNow
			};

			_passwordResetTokenRepository.Create(token);

			var resetUrl = BuildFrontendUrl(
				_frontendOptions.BaseUrl,
				_frontendOptions.PasswordResetPath,
				$"token={Uri.EscapeDataString(rawToken)}");

			await _emailService.SendAsync(
				user.Email,
				"Nulstil password",
				$"Aabn dette link for at nulstille dit password: {resetUrl}"
			);
		}

		public void ResetPassword(string token, string newPassword)
		{
			if (string.IsNullOrWhiteSpace(token))
				throw new ValidationException("Token kræves.");

			if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
				throw new ValidationException("Password skal være mindst 8 tegn.");

			var tokenHash = Security.TokenHasher.Hash(token);

			var resetToken = _passwordResetTokenRepository.GetActiveByHash(tokenHash);
			if (resetToken == null)
				throw new NotFoundException("Ugyldigt eller brugt token");

			if (resetToken.ExpiresAt < DateTime.UtcNow)
				throw new ValidationException("Token er udløbet");

			var user = _userRepository.GetById(resetToken.UserId);
			if (user == null)
				throw new NotFoundException("Bruger ikke fundet");

			user.PasswordHash = _passwordService.HashPassword(newPassword);

			var updated = _userRepository.Update(user);
			if (!updated)
				throw new ConflictException("Password kunne ikke opdateres.");

			_passwordResetTokenRepository.MarkAsUsed(resetToken.Id);
			_refreshTokenRepository.RevokeAllForUser(user.UserId);
		}

		private AuthTokenResponse IssueSession(User user)
		{
			var rawRefreshToken = _jwtService.GenerateRefreshToken();
			var refreshTokenHash = Security.TokenHasher.Hash(rawRefreshToken);
			var refreshToken = new RefreshToken
			{
				UserId = user.UserId,
				TokenHash = refreshTokenHash,
				CreatedAt = DateTime.UtcNow,
				ExpiresAt = DateTime.UtcNow.AddDays(_authOptions.RefreshTokenLifetimeDays)
			};

			_refreshTokenRepository.Create(refreshToken);

			return BuildAuthTokenResponse(user, rawRefreshToken, refreshToken.ExpiresAt);
		}

		private AuthTokenResponse BuildAuthTokenResponse(User user, string rawRefreshToken, DateTime refreshTokenExpiresAt)
		{
			return new AuthTokenResponse
			{
				AccessToken = _jwtService.GenerateToken(user),
				AccessTokenExpiresAt = _jwtService.GetAccessTokenExpiresAtUtc(),
				RefreshToken = rawRefreshToken,
				RefreshTokenExpiresAt = refreshTokenExpiresAt,
				User = new AuthenticatedUserResponse
				{
					UserId = user.UserId,
					Email = user.Email,
					Role = user.Role.ToString()
				}
			};
		}

		private void EnsureUserCanAuthenticate(User user)
		{
			if (!user.IsActive)
				throw new UnauthorizedException("Brugeren er deaktiveret.");

			if (_authOptions.RequireConfirmedEmail && !user.EmailConfirmed)
				throw new UnauthorizedException("Din email er ikke bekræftet.");
		}

		private static string BuildFrontendUrl(string baseUrl, string path, string? query = null)
		{
			var trimmedBaseUrl = baseUrl.TrimEnd('/');
			var normalizedPath = path.StartsWith('/') ? path : $"/{path}";
			var url = $"{trimmedBaseUrl}{normalizedPath}";

			if (!string.IsNullOrWhiteSpace(query))
			{
				url = $"{url}?{query}";
			}

			return url;
		}
	}
}
