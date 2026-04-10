namespace BusRejser.Options
{
	public class RateLimitingOptions
	{
		public const string SectionName = "RateLimiting";

		public RateLimitPolicyOptions Login { get; set; } = new();
		public RateLimitPolicyOptions Register { get; set; } = new();
		public RateLimitPolicyOptions ForgotPassword { get; set; } = new();
		public RateLimitPolicyOptions ResetPassword { get; set; } = new();
		public RateLimitPolicyOptions RefreshToken { get; set; } = new();
		public RateLimitPolicyOptions CheckoutCreate { get; set; } = new();
		public RateLimitPolicyOptions CheckoutStatus { get; set; } = new();
	}
}
