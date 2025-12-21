using BusRejserLibrary.Database;
using BusRejserLibrary.Repositories;
using BusRejserLibrary.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connStr = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connStr))
	throw new Exception("Connection string 'DefaultConnection' mangler.");

builder.Services.AddSingleton(new DBConnection(connStr));

// Repositories
builder.Services.AddScoped(_ => new BusRepository(connStr)); // bruger din eksisterende
builder.Services.AddScoped<FacilitetRepository>();
builder.Services.AddScoped<BusFacilitetRepository>();

// Services
builder.Services.AddScoped<BusService>();
builder.Services.AddScoped<FacilitetService>();

builder.Services.AddSwaggerGen(); 

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.MapControllers();
app.Run();
