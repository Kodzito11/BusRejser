using BusRejserLibrary.Enums;
using BusRejserLibrary.Models;

namespace BusRejser.DTOs
{
	public class FacilitetCreateRequest
	{
		public string Name { get; set; } = "";
		public string Description { get; set; } = "";
		public decimal ExtraPrice { get; set; }
		public bool IsActive { get; set; }
		public FacilitetType Type { get; set; }
	}
}