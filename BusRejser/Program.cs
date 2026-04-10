using System.Text;
using System.Text.Json;
using System.Threading.RateLimiting;
using BusRejser.DTOs;
using BusRejser.Middleware;
using BusRejser.Options;
using BusRejser.Services;
using BusRejserLibrary.Database;
using BusRejserLibrary.Repositories;
using BusRejserLibrary.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
	.ReadFrom.Configuration(builder.Configuration)
	.Enrich.FromLogContext()
	.CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
	options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
	{
		Name = "Authorization",
		Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
		Scheme = "bearer",
		BearerFormat = "JWT",
		In = Microsoft.OpenApi.Models.ParameterLocation.Header,
		Description = "Skriv: Bearer {token}"
	});

	options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
	{
		{
			new Microsoft.OpenApi.Models.OpenApiSecurityScheme
			{
				Reference = new Microsoft.OpenApi.Models.OpenApiReference
				{
					Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
					Id = "Bearer"
				}
			},
			Array.Empty<string>()
		}
	});
});

var connectionStringOptions = builder.Configuration
	.GetSection(ConnectionStringOptions.SectionName)
	.Get<ConnectionStringOptions>() ?? new ConnectionStringOptions();

var jwtOptions = builder.Configuration
	.GetSection(JwtOptions.SectionName)
	.Get<JwtOptions>() ?? new JwtOptions();

var corsOptions = builder.Configuration
	.GetSection(CorsOptions.SectionName)
	.Get<CorsOptions>() ?? new CorsOptions();

var rateLimitingOptions = builder.Configuration
	.GetSection(RateLimitingOptions.SectionName)
	.Get<RateLimitingOptions>() ?? new RateLimitingOptions();

builder.Services.AddOptions<ConnectionStringOptions>()
	.Bind(builder.Configuration.GetSection(ConnectionStringOptions.SectionName))
	.Validate(options => !string.IsNullOrWhiteSpace(options.DefaultConnection), "ConnectionStrings:DefaultConnection mangler.")
	.ValidateOnStart();

builder.Services.AddOptions<JwtOptions>()
	.Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
	.Validate(options => !string.IsNullOrWhiteSpace(options.Secret), "Jwt:Secret mangler.")
	.Validate(options => options.Secret.Trim().Length >= 32, "Jwt:Secret skal vaere mindst 32 tegn.")
	.Validate(options => !string.IsNullOrWhiteSpace(options.Issuer), "Jwt:Issuer mangler.")
	.Validate(options => !string.IsNullOrWhiteSpace(options.Audience), "Jwt:Audience mangler.")
	.Validate(options => options.AccessTokenLifetimeMinutes is >= 5 and <= 120, "Jwt:AccessTokenLifetimeMinutes skal vaere mellem 5 og 120.")
	.ValidateOnStart();

builder.Services.AddOptions<AuthOptions>()
	.Bind(builder.Configuration.GetSection(AuthOptions.SectionName))
	.Validate(options => options.RefreshTokenLifetimeDays is >= 1 and <= 90, "Auth:RefreshTokenLifetimeDays skal vaere mellem 1 og 90.")
	.ValidateOnStart();

builder.Services.AddOptions<StripeOptions>()
	.Bind(builder.Configuration.GetSection(StripeOptions.SectionName))
	.Validate(options => !string.IsNullOrWhiteSpace(options.SecretKey), "Stripe:SecretKey mangler.")
	.Validate(options => !string.IsNullOrWhiteSpace(options.WebhookSecret), "Stripe:WebhookSecret mangler.")
	.ValidateOnStart();

builder.Services.AddOptions<EmailOptions>()
	.Bind(builder.Configuration.GetSection(EmailOptions.SectionName))
	.Validate(options => !string.IsNullOrWhiteSpace(options.Host), "Email:Host mangler.")
	.Validate(options => options.Port is > 0 and <= 65535, "Email:Port skal vaere mellem 1 og 65535.")
	.Validate(options => !string.IsNullOrWhiteSpace(options.Username), "Email:Username mangler.")
	.Validate(options => !string.IsNullOrWhiteSpace(options.Password), "Email:Password mangler.")
	.Validate(options => !string.IsNullOrWhiteSpace(options.From), "Email:From mangler.")
	.ValidateOnStart();

builder.Services.AddOptions<CorsOptions>()
	.Bind(builder.Configuration.GetSection(CorsOptions.SectionName))
	.Validate(options => options.AllowedOrigins.Count > 0, "Cors:AllowedOrigins skal indeholde mindst en origin.")
	.Validate(options => options.AllowedOrigins.All(IsValidAbsoluteHttpUrl), "Alle Cors:AllowedOrigins skal vaere gyldige absolute http/https URLs.")
	.ValidateOnStart();

builder.Services.AddOptions<FrontendOptions>()
	.Bind(builder.Configuration.GetSection(FrontendOptions.SectionName))
	.Validate(options => IsValidAbsoluteHttpUrl(options.BaseUrl), "Frontend:BaseUrl skal vaere en gyldig absolute http/https URL.")
	.Validate(options => !string.IsNullOrWhiteSpace(options.PaymentSuccessPath), "Frontend:PaymentSuccessPath mangler.")
	.Validate(options => !string.IsNullOrWhiteSpace(options.PaymentCancelPath), "Frontend:PaymentCancelPath mangler.")
	.Validate(options => !string.IsNullOrWhiteSpace(options.PasswordResetPath), "Frontend:PasswordResetPath mangler.")
	.ValidateOnStart();

builder.Services.AddOptions<RateLimitingOptions>()
	.Bind(builder.Configuration.GetSection(RateLimitingOptions.SectionName))
	.Validate(options => IsValidRateLimitPolicy(options.Login), "RateLimiting:Login er ugyldig.")
	.Validate(options => IsValidRateLimitPolicy(options.Register), "RateLimiting:Register er ugyldig.")
	.Validate(options => IsValidRateLimitPolicy(options.ForgotPassword), "RateLimiting:ForgotPassword er ugyldig.")
	.Validate(options => IsValidRateLimitPolicy(options.ResetPassword), "RateLimiting:ResetPassword er ugyldig.")
	.Validate(options => IsValidRateLimitPolicy(options.RefreshToken), "RateLimiting:RefreshToken er ugyldig.")
	.Validate(options => IsValidRateLimitPolicy(options.CheckoutCreate), "RateLimiting:CheckoutCreate er ugyldig.")
	.Validate(options => IsValidRateLimitPolicy(options.CheckoutStatus), "RateLimiting:CheckoutStatus er ugyldig.")
	.ValidateOnStart();

builder.Services.AddCors(options =>
{
	options.AddPolicy("frontend", policy =>
		policy.WithOrigins(corsOptions.AllowedOrigins.ToArray())
			.AllowAnyHeader()
			.AllowAnyMethod());
});

builder.Services.AddRateLimiter(options =>
{
	options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
	options.OnRejected = async (context, cancellationToken) =>
	{
		context.HttpContext.Response.ContentType = "application/json";

		if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
		{
			context.HttpContext.Response.Headers.RetryAfter = Math.Ceiling(retryAfter.TotalSeconds).ToString();
		}

		var payload = JsonSerializer.Serialize(new ErrorResponse
		{
			Message = "For mange forespoergsler. Proev igen senere."
		});

		await context.HttpContext.Response.WriteAsync(payload, cancellationToken);
	};

	options.AddPolicy("auth-login", context => CreatePerClientLimiter(
		context,
		rateLimitingOptions.Login,
		"auth-login"));

	options.AddPolicy("auth-register", context => CreatePerClientLimiter(
		context,
		rateLimitingOptions.Register,
		"auth-register"));

	options.AddPolicy("auth-forgot-password", context => CreatePerClientLimiter(
		context,
		rateLimitingOptions.ForgotPassword,
		"auth-forgot-password"));

	options.AddPolicy("auth-reset-password", context => CreatePerClientLimiter(
		context,
		rateLimitingOptions.ResetPassword,
		"auth-reset-password"));

	options.AddPolicy("auth-refresh", context => CreatePerClientLimiter(
		context,
		rateLimitingOptions.RefreshToken,
		"auth-refresh"));

	options.AddPolicy("payment-checkout-create", context => CreatePerClientLimiter(
		context,
		rateLimitingOptions.CheckoutCreate,
		"payment-checkout-create"));

	options.AddPolicy("payment-checkout-status", context => CreatePerClientLimiter(
		context,
		rateLimitingOptions.CheckoutStatus,
		"payment-checkout-status"));
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(options =>
	{
		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidateAudience = true,
			ValidateLifetime = true,
			ValidateIssuerSigningKey = true,
			ValidIssuer = jwtOptions.Issuer,
			ValidAudience = jwtOptions.Audience,
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
			ClockSkew = TimeSpan.FromMinutes(1)
		};

		options.Events = new JwtBearerEvents
		{
			OnChallenge = async context =>
			{
				context.HandleResponse();
				context.Response.StatusCode = StatusCodes.Status401Unauthorized;
				context.Response.ContentType = "application/json";

				var payload = JsonSerializer.Serialize(new ErrorResponse
				{
					Message = "Ikke autoriseret."
				});

				await context.Response.WriteAsync(payload);
			},
			OnForbidden = async context =>
			{
				context.Response.StatusCode = StatusCodes.Status403Forbidden;
				context.Response.ContentType = "application/json";

				var payload = JsonSerializer.Serialize(new ErrorResponse
				{
					Message = "Adgang naegtet."
				});

				await context.Response.WriteAsync(payload);
			}
		};
	});

builder.Services.AddDbContext<BusPlanenDbContext>(options =>
	options.UseMySql(
		connectionStringOptions.DefaultConnection,
		ServerVersion.AutoDetect(connectionStringOptions.DefaultConnection))
);

builder.Services.AddScoped<BusRepository>();

builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<FacilitetRepository>();
builder.Services.AddScoped<BusFacilitetRepository>();

builder.Services.AddScoped<RejseRepository>();
builder.Services.AddScoped<IRejseRepository, RejseRepository>();

builder.Services.AddScoped<BookingRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();

builder.Services.AddScoped<PasswordResetTokenRepository>();
builder.Services.AddScoped<RefreshTokenRepository>();

builder.Services.AddScoped<BusService>();
builder.Services.AddScoped<FacilitetService>();
builder.Services.AddScoped<RejseService>();
builder.Services.AddScoped<PasswordService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<BookingService>();
builder.Services.AddScoped<IStripeCheckoutSessionClient, StripeCheckoutSessionClient>();
builder.Services.AddScoped<StripeService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<UserService>();

var app = builder.Build();

app.UseSerilogRequestLogging(options =>
{
	options.MessageTemplate =
		"HTTP {RequestMethod} {RequestPath} ? {StatusCode} in {Elapsed:0.0000} ms";

	options.EnrichDiagnosticContext = (ctx, http) =>
	{
		ctx.Set("Host", http.Request.Host.Value);
		ctx.Set("UserAgent", http.Request.Headers.UserAgent.ToString());

		if (http.Items.TryGetValue("CorrelationId", out var cid))
		{
			ctx.Set("CorrelationId", cid);
		}
	};
});

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseCors("frontend");
app.UseRateLimiter();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

static bool IsValidAbsoluteHttpUrl(string? url)
{
	return Uri.TryCreate(url, UriKind.Absolute, out var uri)
		&& (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}

static bool IsValidRateLimitPolicy(RateLimitPolicyOptions options)
{
	return options.PermitLimit > 0
		&& options.WindowSeconds > 0
		&& options.QueueLimit >= 0;
}

static RateLimitPartition<string> CreatePerClientLimiter(
	HttpContext context,
	RateLimitPolicyOptions policy,
	string policyName)
{
	var partitionKey = $"{policyName}:{GetRateLimitClientKey(context)}";

	return RateLimitPartition.GetFixedWindowLimiter(
		partitionKey,
		_ => new FixedWindowRateLimiterOptions
		{
			PermitLimit = policy.PermitLimit,
			Window = TimeSpan.FromSeconds(policy.WindowSeconds),
			QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
			QueueLimit = policy.QueueLimit,
			AutoReplenishment = true
		});
}

static string GetRateLimitClientKey(HttpContext context)
{
	var authenticatedUserId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
	if (!string.IsNullOrWhiteSpace(authenticatedUserId))
	{
		return $"user:{authenticatedUserId}";
	}

	return $"ip:{context.Connection.RemoteIpAddress?.ToString() ?? "unknown"}";
}
