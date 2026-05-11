using BusRejser.Features.Progression.DTOs;
using BusRejser.Features.Progression.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusRejser.Features.Progression
{
	[ApiController]
	[Route("api/admin/progression/territories")]
	[Authorize(Roles = "Admin,Medarbejder")]
	public class ProgressionTerritoryAdminController : ControllerBase
	{
		private readonly ProgressionTerritoryAdminService _service;

		public ProgressionTerritoryAdminController(
			ProgressionTerritoryAdminService service)
		{
			_service = service;
		}

		[HttpGet]
		public ActionResult<List<ProgressionTerritoryAdminResponse>> GetAll()
		{
			return Ok(_service.GetAll());
		}

		[HttpGet("{id:int}")]
		public ActionResult<ProgressionTerritoryAdminResponse> GetById(int id)
		{
			return Ok(_service.GetById(id));
		}

		[HttpPost]
		public IActionResult Create(CreateProgressionTerritoryRequest request)
		{
			var id = _service.Create(request);

			return CreatedAtAction(
				nameof(GetById),
				new { id },
				new { id }
			);
		}

		[HttpPut("{id:int}")]
		public IActionResult Update(
			int id,
			UpdateProgressionTerritoryRequest request)
		{
			_service.Update(id, request);

			return NoContent();
		}

		[HttpPost("{id:int}/aliases")]
		public IActionResult AddAlias(
			int id,
			AddProgressionTerritoryAliasRequest request)
		{
			_service.AddAlias(id, request);

			return NoContent();
		}

		[HttpDelete("aliases/{aliasId:int}")]
		public IActionResult RemoveAlias(int aliasId)
		{
			_service.RemoveAlias(aliasId);

			return NoContent();
		}
	}
}