using BusRejser.DTOs;
using BusRejser.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

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
		[EnableRateLimiting("auth-register")]
		public ActionResult<RegisterResponse> Register([FromBody] RegisterRequest request)
		{
			var userId = _authService.Register(request.FirstName, request.LastName, request.Email, request.Password);

			return Ok(new RegisterResponse
			{
				Message = "Bruger oprettet.",
				UserId = userId
			});
		}

		[HttpPost("login")]
		[EnableRateLimiting("auth-login")]
		public ActionResult<AuthTokenResponse> Login([FromBody] LoginRequest request)
		{
			var response = _authService.Login(request.Email, request.Password);
			return Ok(response);
		}

		[HttpPost("refresh")]
		[EnableRateLimiting("auth-refresh")]
		public ActionResult<AuthTokenResponse> Refresh([FromBody] RefreshTokenRequest request)
		{
			var response = _authService.Refresh(request.RefreshToken);
			return Ok(response);
		}

		[HttpPost("logout")]
		public ActionResult<AuthMessageResponse> Logout([FromBody] LogoutRequest request)
		{
			_authService.Logout(request.RefreshToken);
			return Ok(new AuthMessageResponse
			{
				Message = "Session afsluttet."
			});
		}

		[HttpPost("forgot-password")]
		[EnableRateLimiting("auth-forgot-password")]
		public async Task<ActionResult<AuthMessageResponse>> ForgotPassword([FromBody] ForgotPasswordRequest request)
		{
			await _authService.ForgotPassword(request.Email);
			return Ok(new AuthMessageResponse
			{
				Message = "Hvis email findes, er link sendt."
			});
		}

		[HttpPost("reset-password")]
		[EnableRateLimiting("auth-reset-password")]
		public ActionResult<AuthMessageResponse> ResetPassword([FromBody] ResetPasswordRequest request)
		{
			_authService.ResetPassword(request.Token, request.NewPassword);
			return Ok(new AuthMessageResponse
			{
				Message = "Password opdateret."
			});
		}
	}
}
