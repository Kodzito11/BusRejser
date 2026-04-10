using BusRejser.Exceptions;
using BusRejserLibrary.Models;
using BusRejserLibrary.Repositories;

namespace BusRejser.Services
{
	public class AuthService
	{
		private readonly UserRepository _userRepository;
		private readonly PasswordService _passwordService;
		private readonly JwtService _jwtService;
		private readonly PasswordResetTokenRepository _passwordResetTokenRepository;
		private readonly EmailService _emailService;

		public AuthService(
			UserRepository userRepository,
			PasswordService passwordService,
			JwtService jwtService,
			PasswordResetTokenRepository passwordResetTokenRepository,
			EmailService emailService)
		{
			_userRepository = userRepository;
			_passwordService = passwordService;
			_jwtService = jwtService;
			_passwordResetTokenRepository = passwordResetTokenRepository;
			_emailService = emailService;
		}

		public int Register(string username, string email, string password)
		{
			if (string.IsNullOrWhiteSpace(username))
				throw new ValidationException("Brugernavn kræves.");

			if (string.IsNullOrWhiteSpace(email))
				throw new ValidationException("Email kræves.");

			if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
				throw new ValidationException("Password skal være mindst 8 tegn.");

			username = username.Trim();
			email = email.Trim().ToLowerInvariant();

			var existingEmail = _userRepository.GetByEmail(email);
			if (existingEmail != null)
				throw new ConflictException("Email findes allerede.");

			var existingUsername = _userRepository.GetByUsername(username);
			if (existingUsername != null)
				throw new ConflictException("Brugernavn findes allerede.");

			var passwordHash = _passwordService.HashPassword(password);

			var user = new User
			{
				Username = username,
				Email = email,
				PasswordHash = passwordHash,
				Role = BusRejserLibrary.Enums.UserRole.Kunde,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			};

			return _userRepository.Create(user);
		}

		public string Login(string email, string password)
		{
			if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
				throw new UnauthorizedException("Forkert email eller password.");

			email = email.Trim().ToLowerInvariant();

			var user = _userRepository.GetByEmail(email);
			if (user == null)
				throw new UnauthorizedException("Forkert email eller password.");

			if (!user.IsActive)
				throw new UnauthorizedException("Brugeren er deaktiveret.");

			var isValid = _passwordService.VerifyPassword(password, user.PasswordHash);
			if (!isValid)
				throw new UnauthorizedException("Forkert email eller password.");

			return _jwtService.GenerateToken(user);
		}

		public async Task ForgotPassword(string email)
		{
			var user = _userRepository.GetByEmail(email);

			if (user == null)
				return;

			_passwordResetTokenRepository.InvalidateAllForUser(user.UserId);

			var rawToken = Guid.NewGuid().ToString();
			var tokenHash = Security.TokenHasher.Hash(rawToken);

			var token = new BusRejserLibrary.Models.PasswordResetToken
			{
				UserId = user.UserId,
				TokenHash = tokenHash,
				ExpiresAt = DateTime.UtcNow.AddMinutes(30),
				CreatedAt = DateTime.UtcNow
			};

			_passwordResetTokenRepository.Create(token);

			try
			{
				await _emailService.SendAsync(
					user.Email,
					"Nulstil password",
					$"http://localhost:5173/reset-password?token={rawToken}"
				);

				Console.WriteLine("MAIL SENDT OK");
			}
			catch (Exception ex)
			{
				Console.WriteLine("MAIL FEJL: " + ex.Message);
				throw;
			}
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
		}
	}
}
