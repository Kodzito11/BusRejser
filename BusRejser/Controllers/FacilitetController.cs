using BusRejser.DTOs;
using BusRejser.Mappers;
using BusRejserLibrary.Models;
using BusRejserLibrary.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusRejser.Controllers
{
	[ApiController]
	[Authorize]
	[Route("api/[controller]")]
	public class FacilitetController : ControllerBase
	{
		private readonly FacilitetService _facilitetService;

		public FacilitetController(FacilitetService facilitetService)
		{
			_facilitetService = facilitetService;
		}

		[HttpGet]
		[AllowAnonymous]
		public ActionResult<IEnumerable<FacilitetResponse>> GetAll()
		{
			var faciliteter = _facilitetService.GetAll();
			return Ok(faciliteter.Select(FacilitetMapper.ToResponse));
		}

		[HttpPost]
		[Authorize(Roles = "Admin,Medarbejder")]
		public ActionResult<int> Create([FromBody] FacilitetCreateRequest request)
		{
			var facilitet = Facilitet.Create(
				request.Name,
				request.Description,
				request.ExtraPrice,
				request.Type,
				request.IsActive
			);

			var id = _facilitetService.Create(facilitet);
			return Ok(id);
		}

		[HttpDelete("{id:int}")]
		[Authorize(Roles = "Admin,Medarbejder")]
		public ActionResult Delete(int id)
		{
			var ok = _facilitetService.Delete(id);

			return ok
				? Ok()
				: NotFound(new ErrorResponse { Message = "Facilitet blev ikke fundet." });
		}
	}
}