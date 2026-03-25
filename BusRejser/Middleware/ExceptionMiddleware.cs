using System.Net;
using System.Text.Json;
using BusRejser.DTOs;

namespace BusRejser.Middleware
{
	public class ExceptionMiddleware
	{
		private readonly RequestDelegate _next;

		public ExceptionMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			try
			{
				await _next(context);
			}
			catch (Exception ex)
			{
				await HandleExceptionAsync(context, ex);
			}
		}

		private static Task HandleExceptionAsync(HttpContext context, Exception ex)
		
		{
			var statusCode = HttpStatusCode.InternalServerError;

			var message = ex.Message.ToLower();

			if (message.Contains("findes ikke"))
				statusCode = HttpStatusCode.NotFound;

			else if (message.Contains("ikke nok"))
				statusCode = HttpStatusCode.BadRequest;

			else if (message.Contains("ugyldig"))
				statusCode = HttpStatusCode.BadRequest;

			else if (message.Contains("allerede"))
				statusCode = HttpStatusCode.BadRequest;

			else if (message.Contains("forkert") || message.Contains("password"))
				statusCode = HttpStatusCode.Unauthorized;

			else if (message.Contains("må kun"))
				statusCode = HttpStatusCode.Forbidden;

			var response = new ErrorResponse
			{
				Message = ex.Message
			};

			var json = JsonSerializer.Serialize(response);

			context.Response.ContentType = "application/json";
			context.Response.StatusCode = (int)statusCode;

			return context.Response.WriteAsync(json);
		}
	}
}