using BusRejser.DTOs;
using BusRejser.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusRejser.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class GeoController : ControllerBase
	{
		private readonly GeoLookupService _geoLookupService;

		public GeoController(GeoLookupService geoLookupService)
		{
			_geoLookupService = geoLookupService;
		}

		[HttpGet("search")]
		public ActionResult<IEnumerable<GeoNamePlaceResponse>> Search([FromQuery] string q)
		{
			if (string.IsNullOrWhiteSpace(q))
				return BadRequest(new { message = "Query mangler." });

			return Ok(_geoLookupService.Search(q));
		}
	}
}