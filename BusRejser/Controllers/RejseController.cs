using BusRejser.DTOs;
using BusRejser.Mappers;
using BusRejserLibrary.Models;
using BusRejserLibrary.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusRejser.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class RejseController : ControllerBase
	{
		private readonly RejseService _service;

		public RejseController(RejseService service)
		{
			_service = service;
		}

		[HttpGet]
		[AllowAnonymous]
		public ActionResult<IEnumerable<RejseResponse>> GetAll()
		{
			var rejser = _service.GetAll();
			var result = rejser.Select(RejseMapper.ToResponse);

			return Ok(result);
		}

		[HttpGet("{id:int}")]
		[AllowAnonymous]
		public ActionResult<RejseResponse> GetById(int id)
		{
			var rejse = _service.GetById(id);
			if (rejse == null)
				return NotFound(new ErrorResponse { Message = "Rejse blev ikke fundet." });

			return Ok(RejseMapper.ToResponse(rejse));
		}

		[HttpPost]
		[Authorize(Roles = "Admin,Medarbejder")]
		public ActionResult<int> Create([FromBody] RejseCreateRequest request)
		{
			var rejse = Rejse.Create(
			request.Title,
			request.Destination,
			request.Country,
			request.City,
			request.Region,
			request.Municipality,
			request.StartAt,
			request.EndAt,
			request.Price,
			request.MaxSeats,
			request.BusId,
			request.ShortDescription,
			request.Description,
			request.ImageUrl,
			request.IsFeatured,
			request.IsPublished
		);

			var id = _service.Create(rejse);
			return Ok(id);
		}

		[HttpDelete("{id:int}")]
		[Authorize(Roles = "Admin,Medarbejder")]
		public ActionResult Delete(int id)
		{
			_service.Delete(id);
			return Ok();
		}

		[HttpPut("{id:int}")]
		[Authorize(Roles = "Admin,Medarbejder")]
		public ActionResult Update(int id, [FromBody] RejseCreateRequest request)
		{
			var rejse = Rejse.Create(
				request.Title,
				request.Destination,
				request.Country,
				request.City,
				request.Region,
				request.Municipality,
				request.StartAt,
				request.EndAt,
				request.Price,
				request.MaxSeats,
				request.BusId,
				request.ShortDescription,
				request.Description,
				request.ImageUrl,
				request.IsFeatured,
				request.IsPublished
			);

			_service.Update(id, rejse);
			return Ok();
		}

		[HttpGet("public")]
		[AllowAnonymous]
		public ActionResult<IEnumerable<RejseResponse>> GetPublic()
		{
			var now = DateTime.UtcNow;

			var rejser = _service.GetAll()
				.Where(r => r.IsPublished && r.StartAt > now)
				.Select(RejseMapper.ToResponse);

			return Ok(rejser);
		}

		[HttpGet("public/{id:int}")]
		[AllowAnonymous]
		public ActionResult<RejseResponse> GetPublicById(int id)
		{
			var rejse = _service.GetById(id);

			if (rejse == null || !rejse.IsPublished)
				return NotFound();

			return Ok(RejseMapper.ToResponse(rejse));
		}
	}
}