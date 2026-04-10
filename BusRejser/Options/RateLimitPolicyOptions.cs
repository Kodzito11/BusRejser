namespace BusRejser.Options
{
	public class RateLimitPolicyOptions
	{
		public int PermitLimit { get; set; }
		public int WindowSeconds { get; set; }
		public int QueueLimit { get; set; }
	}
}
