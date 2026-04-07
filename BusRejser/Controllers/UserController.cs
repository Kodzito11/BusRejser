using BusRejser.DTOs;
using BusRejser.Exceptions;
using BusRejser.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BusRejser.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize]
	public class UserController : ControllerBase
	{
		private readonly UserService _userService;

		public UserController(UserService userService)
		{
			_userService = userService;
		}

		[HttpGet("me")]
		public ActionResult<UserProfileResponse> Me()
		{
			var userId = GetUserId();
			return Ok(_userService.GetMe(userId));
		}

		[HttpPut("me")]
		public ActionResult<UserProfileResponse> UpdateMe([FromBody] UpdateMyProfileRequest request)
		{
			var userId = GetUserId();
			return Ok(_userService.UpdateMe(userId, request));
		}

		[HttpPut("change-password")]
		public IActionResult ChangePassword([FromBody] ChangePasswordRequest request)
		{
			var userId = GetUserId();
			_userService.ChangePassword(userId, request);
			return Ok(new { message = "Password opdateret." });
		}

		private int GetUserId()
		{
			var userIdRaw = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			if (!int.TryParse(userIdRaw, out var userId))
				throw new UnauthorizedException("Ugyldig bruger.");

			return userId;
		}
	}


}