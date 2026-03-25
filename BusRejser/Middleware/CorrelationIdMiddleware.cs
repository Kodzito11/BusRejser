using Serilog.Context;

public class CorrelationIdMiddleware
{
	private const string HeaderName = "X-Correlation-Id";
	private readonly RequestDelegate _next;

	public CorrelationIdMiddleware(RequestDelegate next)
	{
		_next = next;
	}

	public async Task Invoke(HttpContext context)
	{
		var correlationId = context.Request.Headers.TryGetValue(HeaderName, out var existing)
			&& !string.IsNullOrWhiteSpace(existing)
				? existing.ToString()
				: Guid.NewGuid().ToString();

		context.Response.Headers[HeaderName] = correlationId;

		using (LogContext.PushProperty("CorrelationId", correlationId))
		{
			context.Items["CorrelationId"] = correlationId;
			await _next(context);
		}
	}
}