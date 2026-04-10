using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace BusRejserLibrary.Database
{
	public class BusPlanenDbContextFactory : IDesignTimeDbContextFactory<BusPlanenDbContext>
	{
		public BusPlanenDbContext CreateDbContext(string[] args)
		{
			var basePath = ResolveBasePath();

			var configuration = new ConfigurationBuilder()
				.SetBasePath(basePath)
				.AddJsonFile("appsettings.json", optional: true)
				.AddJsonFile("appsettings.Development.json", optional: true)
				.AddEnvironmentVariables()
				.Build();

			var connectionString = configuration.GetConnectionString("DefaultConnection");
			if (string.IsNullOrWhiteSpace(connectionString))
			{
				throw new InvalidOperationException("ConnectionStrings:DefaultConnection mangler for design-time DbContext creation.");
			}

			var optionsBuilder = new DbContextOptionsBuilder<BusPlanenDbContext>();
			optionsBuilder.UseMySql(
				connectionString,
				new MySqlServerVersion(new Version(8, 0, 36)));

			return new BusPlanenDbContext(optionsBuilder.Options);
		}

		private static string ResolveBasePath()
		{
			var currentDirectory = Directory.GetCurrentDirectory();
			var startupProjectPath = Path.Combine(currentDirectory, "..", "BusRejser");

			if (Directory.Exists(startupProjectPath))
			{
				return Path.GetFullPath(startupProjectPath);
			}

			return currentDirectory;
		}
	}
}
