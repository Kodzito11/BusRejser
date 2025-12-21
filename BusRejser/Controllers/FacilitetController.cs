using System.Collections.Generic;
using BusRejserLibrary.Models;
using BusRejserLibrary.Services;
using Microsoft.AspNetCore.Mvc;

namespace BusRejser.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class FacilitetController : ControllerBase
	{
		private readonly FacilitetService _facilitetService;

		public FacilitetController(FacilitetService facilitetService)
		{
			_facilitetService = facilitetService;
		}

		[HttpGet]
		public ActionResult<List<Facilitet>> GetAll()
		{
			return _facilitetService.GetAll();
		}

		[HttpPost]
		public ActionResult<int> Create([FromBody] FacilitetCreateRequest request)
		{
			var f = Facilitet.Create(
				request.Name,
				request.Description,
				request.ExtraPrice,
				request.Type,
				request.IsActive
			);

			var id = _facilitetService.Create(f);
			return Ok(id);
		}

		[HttpDelete("{id:int}")]
		public ActionResult Delete(int id)
		{
			var ok = _facilitetService.Delete(id);
			return ok ? Ok() : NotFound();
		}
	}

	public class FacilitetCreateRequest
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public decimal ExtraPrice { get; set; }
		public bool IsActive { get; set; }
		public FacilitetType Type { get; set; }
	}
}
