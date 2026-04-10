using BusRejser.DTOs;
using BusRejser.Exceptions;
using BusRejserLibrary.Repositories;

namespace BusRejser.Services
{
	public class UserService
	{
		private readonly UserRepository _userRepository;
		private readonly PasswordService _passwordService;
		private readonly RefreshTokenRepository _refreshTokenRepository;

		public UserService(
			UserRepository userRepository,
			PasswordService passwordService,
			RefreshTokenRepository refreshTokenRepository)
		{
			_userRepository = userRepository;
			_passwordService = passwordService;
			_refreshTokenRepository = refreshTokenRepository;
		}

		public UserProfileResponse GetMe(int userId)
		{
			var user = _userRepository.GetById(userId);
			if (user == null)
				throw new NotFoundException("Bruger ikke fundet.");

			return new UserProfileResponse
			{
				UserId = user.UserId,
				Username = user.Username,
				Email = user.Email,
				Role = user.Role.ToString(),
				CreatedAt = user.CreatedAt,
				FullName = user.FullName,
				Phone = user.PhoneNumber
			};
		}

		public UserProfileResponse UpdateMe(int userId, UpdateMyProfileRequest request)
		{
			var user = _userRepository.GetById(userId);
			if (user == null)
				throw new NotFoundException("Bruger ikke fundet.");

			if (string.IsNullOrWhiteSpace(request.Username))
				throw new ValidationException("Brugernavn kræves.");

			if (string.IsNullOrWhiteSpace(request.Email))
				throw new ValidationException("Email kræves.");

			var normalizedUsername = request.Username.Trim();
			var normalizedEmail = request.Email.Trim().ToLowerInvariant();

			var existingEmail = _userRepository.GetByEmail(normalizedEmail);
			if (existingEmail != null && existingEmail.UserId != userId)
				throw new ConflictException("Email findes allerede.");

			var existingUsername = _userRepository.GetByUsername(normalizedUsername);
			if (existingUsername != null && existingUsername.UserId != userId)
				throw new ConflictException("Brugernavn findes allerede.");

			user.Username = normalizedUsername;
			user.Email = normalizedEmail;
			user.FullName = string.IsNullOrWhiteSpace(request.FullName) ? null : request.FullName.Trim();
			user.PhoneNumber = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();

			var updated = _userRepository.Update(user);
			if (!updated)
				throw new ConflictException("Bruger kunne ikke opdateres.");

			return new UserProfileResponse
			{
				UserId = user.UserId,
				Username = user.Username,
				Email = user.Email,
				Role = user.Role.ToString(),
				CreatedAt = user.CreatedAt,
				FullName = user.FullName,
				Phone = user.PhoneNumber
			};
		}

		public void ChangePassword(int userId, ChangePasswordRequest request)
		{
			var user = _userRepository.GetById(userId);
			if (user == null)
				throw new NotFoundException("Bruger ikke fundet.");

			if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 8)
				throw new ValidationException("Password skal være mindst 8 tegn.");

			var isValid = _passwordService.VerifyPassword(request.CurrentPassword, user.PasswordHash);
			if (!isValid)
				throw new UnauthorizedException("Nuværende password er forkert.");

			user.PasswordHash = _passwordService.HashPassword(request.NewPassword);

			var updated = _userRepository.Update(user);
			if (!updated)
				throw new ConflictException("Password kunne ikke opdateres.");

			_refreshTokenRepository.RevokeAllForUser(user.UserId);
		}
	}
}
