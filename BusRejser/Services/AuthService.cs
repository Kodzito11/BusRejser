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
				CreatedAt = DateTime.UtcNow
			};

			return _userRepository.Create(user);
		}

		public string Login(string email, string password)
		{
			var user = _userRepository.GetByEmail(email);
			if (user == null)
				throw new UnauthorizedException("Forkert email eller password.");

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

			_passwordResetTokenRepository.InvalidateAllForUser(user.Id);

			var rawToken = Guid.NewGuid().ToString();
			var tokenHash = Security.TokenHasher.Hash(rawToken);

			var token = new BusRejserLibrary.Models.PasswordResetToken
			{
				UserId = user.Id,
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

			_userRepository.Update(user);
			_passwordResetTokenRepository.MarkAsUsed(resetToken.Id);
		}
	}
}