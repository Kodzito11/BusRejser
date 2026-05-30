using BusRejser.Common.Logging;
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
		private readonly ILogger<AuthController> _logger;

		public AuthController(
			AuthService authService,
			ILogger<AuthController> logger)
		{
			_authService = authService;
			_logger = logger;
		}

		[HttpPost("register")]
		[EnableRateLimiting("auth-register")]
		public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterRequest request)
		{
			_logger.LogInformation(
				"{EventName} Endpoint={Endpoint}",
				AuthLogEvents.RegisterAttempt,
				"POST /api/auth/register");

			var userId = await _authService.Register(
				request.FirstName,
				request.LastName,
				request.Email,
				request.Password);

			_logger.LogInformation(
				"{EventName} Endpoint={Endpoint} UserId={UserId}",
				AuthLogEvents.RegisterSuccess,
				"POST /api/auth/register",
				userId);

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
			_logger.LogInformation(
				"{EventName} Endpoint={Endpoint}",
				AuthLogEvents.LoginAttempt,
				"POST /api/auth/login");

			var response = _authService.Login(request.Email, request.Password);

			SetRefreshTokenCookie(response.RefreshToken, response.RefreshTokenExpiresAt);

			_logger.LogInformation(
				"{EventName} Endpoint={Endpoint} UserId={UserId} Role={Role} CookieSet={CookieSet}",
				AuthLogEvents.LoginSuccess,
				"POST /api/auth/login",
				response.User.UserId,
				response.User.Role,
				true);

			return Ok(ToSessionResponse(response));
		}

		[HttpPost("refresh")]
		[EnableRateLimiting("auth-refresh")]
		public ActionResult<AuthSessionResponse> Refresh()
		{
			var hasRefreshCookie = Request.Cookies.ContainsKey(RefreshTokenCookieName);

			_logger.LogInformation(
				"{EventName} Endpoint={Endpoint} HasRefreshCookie={HasRefreshCookie}",
				AuthLogEvents.RefreshAttempt,
				"POST /api/auth/refresh",
				hasRefreshCookie);

			var refreshToken = GetRefreshTokenFromCookie();

			if (string.IsNullOrWhiteSpace(refreshToken))
			{
				_logger.LogWarning(
					"{EventName} Endpoint={Endpoint} Reason={Reason}",
					AuthLogEvents.RefreshFailed,
					"POST /api/auth/refresh",
					"MissingRefreshCookie");

				return Unauthorized(new AuthMessageResponse
				{
					Message = "Refresh token mangler."
				});
			}

			var response = _authService.Refresh(refreshToken);

			SetRefreshTokenCookie(response.RefreshToken, response.RefreshTokenExpiresAt);

			_logger.LogInformation(
				"{EventName} Endpoint={Endpoint} UserId={UserId} Role={Role} CookieRotated={CookieRotated}",
				AuthLogEvents.RefreshSuccess,
				"POST /api/auth/refresh",
				response.User.UserId,
				response.User.Role,
				true);

			return Ok(ToSessionResponse(response));
		}

		[HttpPost("logout")]
		public ActionResult<AuthMessageResponse> Logout()
		{
			var hasRefreshCookie = Request.Cookies.ContainsKey(RefreshTokenCookieName);

			_logger.LogInformation(
				"{EventName} Endpoint={Endpoint} HasRefreshCookie={HasRefreshCookie}",
				AuthLogEvents.LogoutAttempt,
				"POST /api/auth/logout",
				hasRefreshCookie);

			var refreshToken = GetRefreshTokenFromCookie();

			if (!string.IsNullOrWhiteSpace(refreshToken))
			{
				_authService.Logout(refreshToken);
			}
			else
			{
				_logger.LogInformation(
					"{EventName} Endpoint={Endpoint} Reason={Reason}",
					AuthLogEvents.LogoutSkipped,
					"POST /api/auth/logout",
					"MissingRefreshCookie");
			}

			DeleteRefreshTokenCookie();

			_logger.LogInformation(
				"{EventName} Endpoint={Endpoint} CookieCleared={CookieCleared}",
				AuthLogEvents.LogoutSuccess,
				"POST /api/auth/logout",
				true);

			return Ok(new AuthMessageResponse
			{
				Message = "Session afsluttet."
			});
		}

		[HttpPost("forgot-password")]
		[EnableRateLimiting("auth-forgot-password")]
		public async Task<ActionResult<AuthMessageResponse>> ForgotPassword([FromBody] ForgotPasswordRequest request)
		{
			_logger.LogInformation(
				"{EventName} Endpoint={Endpoint}",
				AuthLogEvents.PasswordResetRequested,
				"POST /api/auth/forgot-password");

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

			_logger.LogInformation(
				"{EventName} Endpoint={Endpoint}",
				AuthLogEvents.PasswordResetSuccess,
				"POST /api/auth/reset-password");

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

			_logger.LogInformation(
				"{EventName} Endpoint={Endpoint}",
				AuthLogEvents.EmailVerificationSuccess,
				"POST /api/auth/verify-email");

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

			_logger.LogInformation(
				"{EventName} Endpoint={Endpoint}",
				AuthLogEvents.VerificationEmailResent,
				"POST /api/auth/resend-verification-email");

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

			_logger.LogInformation(
				"AUTH_REFRESH_COOKIE_SET ExpiresAt={ExpiresAt} Path={Path} HttpOnly={HttpOnly} Secure={Secure} SameSite={SameSite}",
				expiresAt,
				cookieOptions.Path,
				cookieOptions.HttpOnly,
				cookieOptions.Secure,
				cookieOptions.SameSite);
		}

		private string? GetRefreshTokenFromCookie()
		{
			return Request.Cookies.TryGetValue(RefreshTokenCookieName, out var refreshToken)
				? refreshToken
				: null;
		}

		private void DeleteRefreshTokenCookie()
		{
			var cookieOptions = new CookieOptions
			{
				HttpOnly = true,
				Secure = true,
				SameSite = SameSiteMode.None,
				Path = "/api/auth"
			};

			Response.Cookies.Delete(RefreshTokenCookieName, cookieOptions);

			_logger.LogInformation(
				"AUTH_REFRESH_COOKIE_CLEARED Path={Path} HttpOnly={HttpOnly} Secure={Secure} SameSite={SameSite}",
				cookieOptions.Path,
				cookieOptions.HttpOnly,
				cookieOptions.Secure,
				cookieOptions.SameSite);
		}
	}
}