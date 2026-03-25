using System.Net;
using System.Text.Json;
using BusRejser.DTOs;
using BusRejser.Exceptions;

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
			var statusCode = ex switch
			{
				NotFoundException => HttpStatusCode.NotFound,
				ValidationException => HttpStatusCode.BadRequest,
				UnauthorizedException => HttpStatusCode.Unauthorized,
				ForbiddenException => HttpStatusCode.Forbidden,
				ConflictException => HttpStatusCode.Conflict,
				_ => HttpStatusCode.InternalServerError
			};

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