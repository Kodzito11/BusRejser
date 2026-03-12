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
		[HttpGet("me")]
		public IActionResult Me()
		{
			return Ok(new
			{
				UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
				Username = User.FindFirst(ClaimTypes.Name)?.Value,
				Role = User.FindFirst(ClaimTypes.Role)?.Value,
				Email = User.FindFirst(ClaimTypes.Email)?.Value
			});
		}
	}
}