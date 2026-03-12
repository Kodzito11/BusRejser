using BusRejserLibrary.Models;
using BusRejserLibrary.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

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
		public ActionResult<List<Bus>> GetAll()
		{
			return _busService.GetAll();
		}

		[HttpGet("{id:int}")]
		public ActionResult<Bus> GetById(int id)
		{
			var bus = _busService.GetById(id);
			if (bus == null) return NotFound();
			return bus;
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
				request.Kapasitet
			);

			var newId = _busService.Create(bus);
			return Ok(newId);
		}

		[HttpPut("{id:int}")]
		[Authorize(Roles = "Admin,Medarbejder")]
		public ActionResult Update(int id, [FromBody] BusUpdateRequest request)
		{
			var bus = _busService.GetById(id);
			if (bus == null) return NotFound();

			bus.Registreringnummer = request.Registreringnummer;
			bus.Model = request.Model;
			bus.Busselskab = request.Busselskab;
			bus.Kapasitet = request.Kapasitet;
			bus.SetStatus(request.Status);
			bus.Type = request.Type;

			var ok = _busService.Update(bus);
			return ok ? Ok() : BadRequest();
		}

		[HttpDelete("{id:int}")]
		[Authorize (Roles = "Admin,Medarbejder")]
		public ActionResult Delete(int id)
		{
			var ok = _busService.Delete(id);
			return ok ? Ok() : NotFound();
		}

		[HttpGet("{id:int}/faciliteter")]
		public ActionResult<List<Facilitet>> GetFaciliteter(int id)
		{
			return _busService.GetFaciliteterForBus(id);
		}

		[HttpPost("{id:int}/faciliteter/{facilitetId:int}")]
		[Authorize (Roles = "Admin,Medarbejder")]
		public ActionResult AddFacilitet(int id, int facilitetId)
		{
			var ok = _busService.AddFacilitet(id, facilitetId);
			return ok ? Ok() : BadRequest();
		}

		[HttpDelete("{id:int}/faciliteter/{facilitetId:int}")]
		[Authorize(Roles = "Admin,Medarbejder")]
		public ActionResult RemoveFacilitet(int id, int facilitetId)
		{
			var ok = _busService.RemoveFacilitet(id, facilitetId);
			return ok ? Ok() : NotFound();
		}
	}

	public class BusCreateRequest
	{
		public string Registreringnummer { get; set; }
		public string Model { get; set; }
		public string Busselskab { get; set; }
		public BusStatus Status { get; set; }
		public BusType Type { get; set; }
		public int Kapasitet { get; set; }
	}

	public class BusUpdateRequest : BusCreateRequest { }
}
