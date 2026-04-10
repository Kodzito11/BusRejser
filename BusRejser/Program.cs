using BusRejser.Middleware;
using BusRejser.Options;
using BusRejser.Services;
using BusRejserLibrary.Database;
using BusRejserLibrary.Repositories;
using BusRejserLibrary.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

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

builder.Services.AddOptions<ConnectionStringOptions>()
	.Bind(builder.Configuration.GetSection(ConnectionStringOptions.SectionName))
	.Validate(options => !string.IsNullOrWhiteSpace(options.DefaultConnection), "ConnectionStrings:DefaultConnection mangler.")
	.ValidateOnStart();

builder.Services.AddOptions<JwtOptions>()
	.Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
	.Validate(options => !string.IsNullOrWhiteSpace(options.Secret), "Jwt:Secret mangler.")
	.Validate(options => options.Secret.Trim().Length >= 32, "Jwt:Secret skal vaere mindst 32 tegn.")
	.Validate(options => options.AccessTokenLifetimeHours > 0 && options.AccessTokenLifetimeHours <= 24, "Jwt:AccessTokenLifetimeHours skal vaere mellem 1 og 24.")
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
	.ValidateOnStart();

builder.Services.AddCors(options =>
{
	options.AddPolicy("frontend", policy =>
		policy.WithOrigins(corsOptions.AllowedOrigins.ToArray())
			.AllowAnyHeader()
			.AllowAnyMethod());
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(options =>
	{
		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = false,
			ValidateAudience = false,
			ValidateLifetime = true,
			ValidateIssuerSigningKey = true,
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret))
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
