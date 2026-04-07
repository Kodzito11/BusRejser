using BusRejser.DTOs;
using BusRejser.Exceptions;
using BusRejserLibrary.Repositories;

namespace BusRejser.Services
{
	public class UserService
	{
		private readonly UserRepository _userRepository;
		private readonly PasswordService _passwordService;

		public UserService(UserRepository userRepository, PasswordService passwordService)
		{
			_userRepository = userRepository;
			_passwordService = passwordService;
		}

		public UserProfileResponse GetMe(int userId)
		{
			var user = _userRepository.GetById(userId);
			if (user == null)
				throw new NotFoundException("Bruger ikke fundet.");

			return new UserProfileResponse
			{
				UserId = user.Id,
				Username = user.Username,
				Email = user.Email,
				Role = user.Role.ToString(),
				CreatedAt = user.CreatedAt,
				FullName = user.FullName,
				Phone = user.Phone
			};
		}

		public UserProfileResponse UpdateMe(int userId, UpdateMyProfileRequest request)
		{
			var user = _userRepository.GetById(userId);
			if (user == null)
				throw new NotFoundException("Bruger ikke fundet.");

			var existingEmail = _userRepository.GetByEmail(request.Email);
			if (existingEmail != null && existingEmail.Id != userId)
				throw new ConflictException("Email findes allerede.");

			var existingUsername = _userRepository.GetByUsername(request.Username);
			if (existingUsername != null && existingUsername.Id != userId)
				throw new ConflictException("Brugernavn findes allerede.");

			user.Username = request.Username.Trim();
			user.Email = request.Email.Trim();
			user.FullName = string.IsNullOrWhiteSpace(request.FullName) ? null : request.FullName.Trim();
			user.Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();

			_userRepository.Update(user);

			return new UserProfileResponse
			{
				UserId = user.Id,
				Username = user.Username,
				Email = user.Email,
				Role = user.Role.ToString(),
				CreatedAt = user.CreatedAt,
				FullName = user.FullName,
				Phone = user.Phone
			};
		}

		public void ChangePassword(int userId, ChangePasswordRequest request)
		{
			var user = _userRepository.GetById(userId);
			if (user == null)
				throw new NotFoundException("Bruger ikke fundet.");

			var isValid = _passwordService.VerifyPassword(request.CurrentPassword, user.PasswordHash);
			if (!isValid)
				throw new UnauthorizedException("Nuværende password er forkert.");

			user.PasswordHash = _passwordService.HashPassword(request.NewPassword);

			_userRepository.Update(user);
		}
	}
}
