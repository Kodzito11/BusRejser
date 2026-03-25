using BusRejserLibrary.Enums;
using BusRejserLibrary.Models;

namespace BusRejser.DTOs
{
	public class BusCreateRequest
	{
		public string Registreringnummer { get; set; } = "";
		public string Model { get; set; } = "";
		public string Busselskab { get; set; } = "";
		public BusStatus Status { get; set; }
		public BusType Type { get; set; }
		public int Kapasitet { get; set; }
		public string? ImageUrl { get; set; }
	}
}