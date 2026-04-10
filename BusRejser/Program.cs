using BusRejser.Middleware;
using BusRejser.Services;
using BusRejserLibrary.Database;
using BusRejserLibrary.Repositories;
using BusRejserLibrary.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using Microsoft.EntityFrameworkCore;

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

var connStr = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connStr))
	throw new Exception("Connection string 'DefaultConnection' mangler.");

var jwtSecret = builder.Configuration["Jwt:Secret"];
if (string.IsNullOrWhiteSpace(jwtSecret))
	throw new Exception("Jwt:Secret mangler.");

var stripeSecret = builder.Configuration["Stripe:SecretKey"];
if (string.IsNullOrWhiteSpace(stripeSecret))
	throw new Exception("Stripe:SecretKey mangler.");

// CORS
builder.Services.AddCors(options =>
{
	options.AddPolicy("dev", p =>
		p.AllowAnyOrigin()
		 .AllowAnyHeader()
		 .AllowAnyMethod());
});

// JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(options =>
	{
		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = false,
			ValidateAudience = false,
			ValidateLifetime = true,
			ValidateIssuerSigningKey = true,
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
		};
	});

// DbContext
builder.Services.AddDbContext<BusPlanenDbContext>(options =>
	options.UseMySql(
		builder.Configuration.GetConnectionString("DefaultConnection"),
		ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
	));

// Repositories
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

// Services
builder.Services.AddScoped<BusService>();
builder.Services.AddScoped<FacilitetService>();
builder.Services.AddScoped<RejseService>();
builder.Services.AddScoped<PasswordService>();
builder.Services.AddScoped(_ => new JwtService(jwtSecret));
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

app.UseCors("dev");
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
