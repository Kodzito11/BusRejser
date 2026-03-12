using BusRejserLibrary.Models;
using BusRejserLibrary.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

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
		public ActionResult<List<Rejse>> GetAll()
		{
			return _service.GetAll();
		}

		[HttpGet("{id:int}")]
		[AllowAnonymous]
		public ActionResult<Rejse> GetById(int id)
		{
			var r = _service.GetById(id);
			if (r == null) return NotFound();
			return r;
		}

		[HttpPost]
		[Authorize(Roles = "Admin,Medarbejder")]
		public ActionResult<int> Create([FromBody] RejseCreateRequest request)
		{
			var rejse = Rejse.Create(
				request.Title,
				request.Destination,
				request.StartAt,
				request.EndAt,
				request.Price,
				request.MaxSeats,
				request.BusId
			);

			var id = _service.Create(rejse);
			return Ok(id);
		}

		[HttpDelete("{id:int}")]
		[Authorize(Roles = "Admin,Medarbejder")]
		public ActionResult Delete(int id)
		{
			var ok = _service.Delete(id);
			return ok ? Ok() : NotFound();
		}
	}

	public class RejseCreateRequest
	{
		public string Title { get; set; }
		public string Destination { get; set; }
		public DateTime StartAt { get; set; }
		public DateTime EndAt { get; set; }
		public decimal Price { get; set; }
		public int MaxSeats { get; set; }
		public int? BusId { get; set; }
	}
}