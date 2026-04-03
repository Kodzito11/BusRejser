using BusRejser.DTOs;
using BusRejser.Services;
using Microsoft.AspNetCore.Mvc;

namespace BusRejser.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class AuthController : ControllerBase
	{
		private readonly AuthService _authService;

		public AuthController(AuthService authService)
		{
			_authService = authService;
		}

		[HttpPost("register")]
		public IActionResult Register([FromBody] RegisterRequest request)
		{
			var userId = _authService.Register(request.Username, request.Email, request.Password);

			return Ok(new
			{
				message = "Bruger oprettet.",
				userId
			});
		}

		[HttpPost("login")]
		public IActionResult Login([FromBody] LoginRequest request)
		{
			var token = _authService.Login(request.Email, request.Password);

			return Ok(new
			{
				token
			});
		}

		[HttpPost("forgot-password")]
		public IActionResult ForgotPassword([FromBody] ForgotPasswordRequest request)
		{
			_authService.ForgotPassword(request.Email);
			return Ok(new { message = "Hvis email findes, er link sendt." });
		}

		[HttpPost("reset-password")]
		public IActionResult ResetPassword([FromBody] ResetPasswordRequest request)
		{
			_authService.ResetPassword(request.Token, request.NewPassword);
			return Ok(new { message = "Password opdateret." });
		}
	}
}