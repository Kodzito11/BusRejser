using BusRejser.Features.Auth.DTOs;
using BusRejser.Features.Auth.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BusRejser.Features.Auth
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
		public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterRequest request)
		{
			var userId = await _authService.Register(request.FirstName, request.LastName, request.Email, request.Password);

			return Ok(new RegisterResponse
			{
				Message = "Bruger oprettet.",
				UserId = userId
			});
		}

		[HttpPost("login")]
		[EnableRateLimiting("auth-login")]
		public ActionResult<AuthSessionResponse> Login([FromBody] LoginRequest request)
		{
			var response = _authService.Login(request.Email, request.Password);

			SetRefreshTokenCookie(response.RefreshToken, response.RefreshTokenExpiresAt);

			return Ok(ToSessionResponse(response));
		}

		[HttpPost("refresh")]
		[EnableRateLimiting("auth-refresh")]
		public ActionResult<AuthSessionResponse> Refresh()
		{
			var refreshToken = GetRefreshTokenFromCookie();

			if (string.IsNullOrWhiteSpace(refreshToken))
			{
				return Unauthorized(new AuthMessageResponse
				{
					Message = "Refresh token mangler."
				});
			}

			var response = _authService.Refresh(refreshToken);

			SetRefreshTokenCookie(response.RefreshToken, response.RefreshTokenExpiresAt);

			return Ok(ToSessionResponse(response));
		}

		[HttpPost("logout")]
		public ActionResult<AuthMessageResponse> Logout()
		{
			var refreshToken = GetRefreshTokenFromCookie();

			if (!string.IsNullOrWhiteSpace(refreshToken))
			{
				_authService.Logout(refreshToken);
			}

			DeleteRefreshTokenCookie();

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

		[HttpPost("verify-email")]
		[EnableRateLimiting("auth-verify-email")]
		public ActionResult<AuthMessageResponse> VerifyEmail([FromBody] VerifyEmailRequest request)
		{
			_authService.VerifyEmail(request.Token);

			return Ok(new AuthMessageResponse
			{
				Message = "Email bekræftet."
			});
		}

		[HttpPost("resend-verification-email")]
		[EnableRateLimiting("auth-resend-verification-email")]
		public async Task<ActionResult<AuthMessageResponse>> ResendVerificationEmail([FromBody] ResendVerificationEmailRequest request)
		{
			await _authService.ResendVerificationEmail(request.Email);

			return Ok(new AuthMessageResponse
			{
				Message = "Hvis email findes og ikke er bekræftet, er nyt link sendt."
			});
		}

		private static AuthSessionResponse ToSessionResponse(AuthTokenResponse response)
		{
			return new AuthSessionResponse
			{
				TokenType = response.TokenType,
				AccessToken = response.AccessToken,
				AccessTokenExpiresAt = response.AccessTokenExpiresAt,
				RefreshTokenExpiresAt = response.RefreshTokenExpiresAt,
				User = response.User
			};
		}

		private const string RefreshTokenCookieName = "busplanen_refresh";

		private void SetRefreshTokenCookie(string refreshToken, DateTime expiresAt)
		{
			var cookieOptions = new CookieOptions
			{
				HttpOnly = true,
				Secure = true,
				SameSite = SameSiteMode.None,
				Expires = expiresAt,
				Path = "/api/auth"
			};

			Response.Cookies.Append(RefreshTokenCookieName, refreshToken, cookieOptions);
		}

		private string? GetRefreshTokenFromCookie()
		{
			return Request.Cookies.TryGetValue(RefreshTokenCookieName, out var refreshToken)
				? refreshToken
				: null;
		}

		private void DeleteRefreshTokenCookie()
		{
			Response.Cookies.Delete(RefreshTokenCookieName, new CookieOptions
			{
				HttpOnly = true,
				Secure = true,
				SameSite = SameSiteMode.None,
				Path = "/api/auth"
			});
		}
	}
}
