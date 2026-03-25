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
	public class BusController : ControllerBase
	{
		private readonly BusService _busService;

		public BusController(BusService busService)
		{
			_busService = busService;
		}

		[HttpGet]
		public ActionResult<IEnumerable<BusResponse>> GetAll()
		{
			var buses = _busService.GetAll();
			return Ok(buses.Select(BusMapper.ToResponse));
		}

		[HttpGet("{id:int}")]
		public ActionResult<BusResponse> GetById(int id)
		{
			var bus = _busService.GetById(id);
			if (bus == null)
				return NotFound(new ErrorResponse { Message = "Bus blev ikke fundet." });

			return Ok(BusMapper.ToResponse(bus));
		}

		[HttpPost]
		[Authorize(Roles = "Admin,Medarbejder")]
		public ActionResult<int> Create([FromBody] BusCreateRequest request)
		{
			var bus = Bus.Create(
				request.Registreringnummer,
				request.Model,
				request.Busselskab,
				request.Status,
				request.Type,
				request.Kapasitet,
				request.ImageUrl
			);

			var newId = _busService.Create(bus);
			return Ok(newId);
		}

		[HttpPut("{id:int}")]
		[Authorize(Roles = "Admin,Medarbejder")]
		public ActionResult Update(int id, [FromBody] BusUpdateRequest request)
		{
			var bus = _busService.GetById(id);
			if (bus == null)
				return NotFound(new ErrorResponse { Message = "Bus blev ikke fundet." });

			bus.Registreringnummer = request.Registreringnummer;
			bus.Model = request.Model;
			bus.Busselskab = request.Busselskab;
			bus.Kapasitet = request.Kapasitet;
			bus.SetStatus(request.Status);
			bus.Type = request.Type;

			var ok = _busService.Update(bus);
			return ok
				? Ok()
				: BadRequest(new ErrorResponse { Message = "Bus kunne ikke opdateres." });
		}

		[HttpDelete("{id:int}")]
		[Authorize(Roles = "Admin,Medarbejder")]
		public ActionResult Delete(int id)
		{
			var ok = _busService.Delete(id);
			return ok
				? Ok()
				: NotFound(new ErrorResponse { Message = "Bus blev ikke fundet." });
		}

		[HttpGet("{id:int}/faciliteter")]
		public ActionResult<List<Facilitet>> GetFaciliteter(int id)
		{
			return Ok(_busService.GetFaciliteterForBus(id));
		}

		[HttpPost("{id:int}/faciliteter/{facilitetId:int}")]
		[Authorize(Roles = "Admin,Medarbejder")]
		public ActionResult AddFacilitet(int id, int facilitetId)
		{
			var ok = _busService.AddFacilitet(id, facilitetId);
			return ok
				? Ok()
				: BadRequest(new ErrorResponse { Message = "Facilitet kunne ikke tilføjes til bus." });
		}

		[HttpDelete("{id:int}/faciliteter/{facilitetId:int}")]
		[Authorize(Roles = "Admin,Medarbejder")]
		public ActionResult RemoveFacilitet(int id, int facilitetId)
		{
			var ok = _busService.RemoveFacilitet(id, facilitetId);
			return ok
				? Ok()
				: NotFound(new ErrorResponse { Message = "Bus eller facilitet blev ikke fundet." });
		}

		[HttpPost("{id:int}/image")]
		[Authorize(Roles = "Admin,Medarbejder")]
		public async Task<ActionResult> UploadImage(int id, IFormFile file)
		{
			var imageUrl = await _busService.UploadImageAsync(id, file);

			return Ok(new
			{
				message = "Billede uploadet.",
				imageUrl
			});
		}
	}
}