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
			try
			{
				var userId = _authService.Register(request.Username, request.Email, request.Password);

				return Ok(new
				{
					Message = "Bruger oprettet.",
					UserId = userId
				});
			}
			catch (Exception ex)
			{
				return BadRequest(new { Message = ex.Message });
			}
		}

		[HttpPost("login")]
		public IActionResult Login([FromBody] LoginRequest request)
		{
			try
			{
				var token = _authService.Login(request.Email, request.Password);

				return Ok(new
				{
					Token = token
				});
			}
			catch (Exception ex)
			{
				return Unauthorized(new { Message = ex.Message });
			}
		}
	}

	public class RegisterRequest
	{
		public string Username { get; set; } = "";
		public string Email { get; set; } = "";
		public string Password { get; set; } = "";
	}

	public class LoginRequest
	{
		public string Email { get; set; } = "";
		public string Password { get; set; } = "";
	}
}