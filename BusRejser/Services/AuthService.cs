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

		public AuthService(
			UserRepository userRepository,
			PasswordService passwordService,
			JwtService jwtService)
		{
			_userRepository = userRepository;
			_passwordService = passwordService;
			_jwtService = jwtService;
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
	}
}