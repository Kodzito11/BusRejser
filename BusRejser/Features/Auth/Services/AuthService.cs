using BusRejser.Common.DTOs;
using BusRejser.Common.Email;
using BusRejser.Common.Logging;
using BusRejser.Common.Security;
using BusRejser.Exceptions;
using BusRejser.Features.Auth.DTOs;
using BusRejser.Options;
using BusRejserLibrary.Models;
using BusRejserLibrary.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BusRejser.Features.Auth.Services
{
	public class AuthService
	{
		private readonly UserRepository _userRepository;
		private readonly PasswordService _passwordService;
		private readonly JwtService _jwtService;
		private readonly PasswordResetTokenRepository _passwordResetTokenRepository;
		private readonly RefreshTokenRepository _refreshTokenRepository;
		private readonly EmailVerificationTokenRepository _emailVerificationTokenRepository;
		private readonly EmailService _emailService;
		private readonly ILogger<AuthService> _logger;
		private readonly FrontendOptions _frontendOptions;
		private readonly AuthOptions _authOptions;

		public AuthService(
			UserRepository userRepository,
			PasswordService passwordService,
			JwtService jwtService,
			PasswordResetTokenRepository passwordResetTokenRepository,
			RefreshTokenRepository refreshTokenRepository,
			EmailVerificationTokenRepository emailVerificationTokenRepository,
			EmailService emailService,
			ILogger<AuthService> logger,
			IOptions<FrontendOptions> frontendOptions,
			IOptions<AuthOptions> authOptions)
		{
			_userRepository = userRepository;
			_passwordService = passwordService;
			_jwtService = jwtService;
			_passwordResetTokenRepository = passwordResetTokenRepository;
			_emailVerificationTokenRepository = emailVerificationTokenRepository;
			_refreshTokenRepository = refreshTokenRepository;
			_emailService = emailService;
			_logger = logger;
			_frontendOptions = frontendOptions.Value;
			_authOptions = authOptions.Value;
		}

		public async Task<int> Register(string FirstName, string LastName, string email, string password)
		{
			_logger.LogInformation("{EventName}", AuthLogEvents.RegisterAttempt);

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
			{
				_logger.LogWarning("{EventName} Reason={Reason}", AuthLogEvents.RegisterFailed, "EmailAlreadyExists");
				throw new ConflictException("Email findes allerede.");
			}

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

			var userId = _userRepository.Create(user);

			var createdUser = _userRepository.GetById(userId);
			if (createdUser == null)
			{
				_logger.LogWarning("{EventName} Reason={Reason} UserId={UserId}", AuthLogEvents.RegisterFailed, "CreatedUserNotFound", userId);
				throw new ConflictException("Bruger kunne ikke oprettes korrekt.");
			}

			await SendEmailVerificationAsync(createdUser);

			_logger.LogInformation(
				"{EventName} UserId={UserId} Role={Role}",
				AuthLogEvents.RegisterSuccess,
				createdUser.UserId,
				createdUser.Role);

			return userId;
		}

		public AuthTokenResponse Login(string email, string password)
		{
			_logger.LogInformation("{EventName}", AuthLogEvents.LoginAttempt);

			if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
			{
				_logger.LogWarning("{EventName} Reason={Reason}", AuthLogEvents.LoginFailed, "MissingCredentials");
				throw new UnauthorizedException("Forkert email eller password.");
			}

			email = email.Trim().ToLowerInvariant();

			var user = _userRepository.GetByEmail(email);
			if (user == null)
			{
				_logger.LogWarning("{EventName} Reason={Reason}", AuthLogEvents.LoginFailed, "UserNotFound");
				throw new UnauthorizedException("Forkert email eller password.");
			}

			var isValid = _passwordService.VerifyPassword(password, user.PasswordHash);
			if (!isValid)
			{
				_logger.LogWarning(
					"{EventName} Reason={Reason} UserId={UserId}",
					AuthLogEvents.LoginFailed,
					"InvalidPassword",
					user.UserId);

				throw new UnauthorizedException("Forkert email eller password.");
			}

			EnsureUserCanAuthenticate(user);

			user.LastLoginAt = DateTime.UtcNow;
			var updated = _userRepository.Update(user);
			if (!updated)
				throw new ConflictException("Bruger kunne ikke opdateres.");

			_logger.LogInformation(
				"{EventName} UserId={UserId} Role={Role}",
				AuthLogEvents.LoginSuccess,
				user.UserId,
				user.Role);

			return IssueSession(user);
		}

		public void Logout(string refreshToken)
		{
			_logger.LogInformation("{EventName}", AuthLogEvents.LogoutAttempt);

			if (string.IsNullOrWhiteSpace(refreshToken))
			{
				_logger.LogInformation("{EventName} Reason={Reason}", AuthLogEvents.LogoutSkipped, "MissingRefreshToken");
				return;
			}

			var refreshTokenHash = TokenHasher.Hash(refreshToken);
			var existingToken = _refreshTokenRepository.GetByTokenHash(refreshTokenHash);

			if (existingToken == null || existingToken.RevokedAt != null)
			{
				_logger.LogInformation("{EventName} Reason={Reason}", AuthLogEvents.LogoutSkipped, "TokenNotFoundOrAlreadyRevoked");
				return;
			}

			_refreshTokenRepository.Revoke(existingToken);

			_logger.LogInformation(
				"{EventName} UserId={UserId}",
				AuthLogEvents.LogoutSuccess,
				existingToken.UserId);
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
			var tokenHash = TokenHasher.Hash(rawToken);

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

			await _emailService.SendPasswordResetAsync(user.Email, resetUrl);
		}

		public void ResetPassword(string token, string newPassword)
		{
			if (string.IsNullOrWhiteSpace(token))
				throw new ValidationException("Token kræves.");

			if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
				throw new ValidationException("Password skal være mindst 8 tegn.");

			var tokenHash = TokenHasher.Hash(token);

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
			var refreshTokenHash = TokenHasher.Hash(rawRefreshToken);
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

		public AuthTokenResponse Refresh(string refreshToken)
		{
			_logger.LogInformation("{EventName}", AuthLogEvents.RefreshAttempt);

			if (string.IsNullOrWhiteSpace(refreshToken))
			{
				_logger.LogWarning("{EventName} Reason={Reason}", AuthLogEvents.RefreshFailed, "MissingRefreshToken");
				throw new UnauthorizedException("Refresh token mangler.");
			}

			var refreshTokenHash = TokenHasher.Hash(refreshToken);
			var existingToken = _refreshTokenRepository.GetByTokenHash(refreshTokenHash);

			if (existingToken == null || !existingToken.IsActive)
			{
				_logger.LogWarning("{EventName} Reason={Reason}", AuthLogEvents.RefreshFailed, "InvalidOrExpiredToken");
				throw new UnauthorizedException("Refresh token er ugyldig eller udløbet.");
			}

			var user = _userRepository.GetById(existingToken.UserId);
			if (user == null)
			{
				_logger.LogWarning(
					"{EventName} Reason={Reason} UserId={UserId}",
					AuthLogEvents.RefreshFailed,
					"UserNotFound",
					existingToken.UserId);

				throw new UnauthorizedException("Sessionen er ikke længere gyldig.");
			}

			EnsureUserCanAuthenticate(user);

			var rawNewRefreshToken = _jwtService.GenerateRefreshToken();
			var newRefreshTokenHash = TokenHasher.Hash(rawNewRefreshToken);
			var newRefreshToken = new RefreshToken
			{
				UserId = user.UserId,
				TokenHash = newRefreshTokenHash,
				CreatedAt = DateTime.UtcNow,
				ExpiresAt = DateTime.UtcNow.AddDays(_authOptions.RefreshTokenLifetimeDays)
			};

			_refreshTokenRepository.Rotate(existingToken, newRefreshToken);

			_logger.LogInformation(
				"{EventName} UserId={UserId} Role={Role}",
				AuthLogEvents.RefreshSuccess,
				user.UserId,
				user.Role);

			return BuildAuthTokenResponse(user, rawNewRefreshToken, newRefreshToken.ExpiresAt);
		}

		public void VerifyEmail(string token)
		{
			if (string.IsNullOrWhiteSpace(token))
				throw new ValidationException("Token kræves.");

			var tokenHash = TokenHasher.Hash(token);

			var verificationToken = _emailVerificationTokenRepository.GetActiveByHash(tokenHash);
			if (verificationToken == null)
				throw new NotFoundException("Ugyldigt eller brugt token.");

			var user = _userRepository.GetById(verificationToken.UserId);
			if (user == null)
				throw new NotFoundException("Bruger ikke fundet.");

			user.EmailConfirmed = true;
			user.UpdatedAt = DateTime.UtcNow;

			var updated = _userRepository.Update(user);
			if (!updated)
				throw new ConflictException("Email kunne ikke bekræftes.");

			_emailVerificationTokenRepository.MarkAsUsed(
				verificationToken.EmailVerificationTokenId);
		}

		public async Task ResendVerificationEmail(string email)
		{
			if (string.IsNullOrWhiteSpace(email))
				return;

			var normalizedEmail = email.Trim().ToLowerInvariant();
			var user = _userRepository.GetByEmail(normalizedEmail);

			if (user == null)
				return;

			if (user.EmailConfirmed)
				return;

			await SendEmailVerificationAsync(user);
		}

		private void EnsureUserCanAuthenticate(User user)
		{
			if (user.Role == BusRejserLibrary.Enums.UserRole.None)
				throw new UnauthorizedException("Bruger mangler gyldig rolle.");

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

		private async Task SendEmailVerificationAsync(User user)
		{
			_emailVerificationTokenRepository.InvalidateAllForUser(user.UserId);

			var rawToken = Guid.NewGuid().ToString();
			var tokenHash = TokenHasher.Hash(rawToken);

			var token = new EmailVerificationToken
			{
				UserId = user.UserId,
				TokenHash = tokenHash,
				ExpiresAt = DateTime.UtcNow.AddHours(24),
				CreatedAt = DateTime.UtcNow
			};

			_emailVerificationTokenRepository.Create(token);

			var verificationUrl = BuildFrontendUrl(
				_frontendOptions.BaseUrl,
				"/verify-email",
				$"token={Uri.EscapeDataString(rawToken)}");

			await _emailService.SendEmailVerificationAsync(user.Email, verificationUrl);
		}
	}
}
