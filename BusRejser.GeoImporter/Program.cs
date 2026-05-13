using MySqlConnector;
using System.Globalization;

var connectionString = "server=localhost;port=3307;database=busplanen;user=bus_user;password=nyt_password;";

var basePath = @"C:\Users\Kodo1\source\repos\Kodzito11\busrejser\data\geonames";
var mode = args.FirstOrDefault()?.ToLowerInvariant();

if (mode == "places")
{
	await ImportPlaces(connectionString, Path.Combine(basePath, "cities15000.txt"));
}
else if (mode == "dk")
{
	await ImportPlaces(connectionString, Path.Combine(basePath, "DK.txt"));
}
else if (mode == "alternate")
{
	await ImportAlternateNames(connectionString, Path.Combine(basePath, "alternateNamesV2.txt"));
}
else if (mode == "admin2")
{
	await ImportAdmin2Codes(connectionString, Path.Combine(basePath, "admin2Codes.txt"));
}
else
{
	Console.WriteLine("Brug:");
	Console.WriteLine(@"dotnet run --project .\BusRejser.GeoImporter -- places");
	Console.WriteLine(@"dotnet run --project .\BusRejser.GeoImporter -- dk");
	Console.WriteLine(@"dotnet run --project .\BusRejser.GeoImporter -- alternate");
	Console.WriteLine(@"dotnet run --project .\BusRejser.GeoImporter -- admin2");
}

static async Task ImportAdmin2Codes(string connectionString, string filePath)
{
	if (!File.Exists(filePath))
	{
		Console.WriteLine($"Filen findes ikke: {filePath}");
		return;
	}

	await using var connection = new MySqlConnection(connectionString);
	await connection.OpenAsync();

	Console.WriteLine("Forbundet til database.");
	Console.WriteLine("Starter import af admin2 codes...");

	var imported = 0;
	var skipped = 0;

	await using var transaction = await connection.BeginTransactionAsync();

	foreach (var line in File.ReadLines(filePath))
	{
		if (string.IsNullOrWhiteSpace(line))
			continue;

		var cols = line.Split('\t');

		if (cols.Length < 4)
		{
			skipped++;
			continue;
		}

		var code = cols[0];
		var name = cols[1];
		var asciiName = cols[2];

		int? geoNameId = null;
		if (int.TryParse(cols[3], out var parsedGeoNameId))
			geoNameId = parsedGeoNameId;

		var sql = """
            INSERT INTO geo_admin2_codes
            (
                Code,
                Name,
                AsciiName,
                GeoNameId
            )
            VALUES
            (
                @Code,
                @Name,
                @AsciiName,
                @GeoNameId
            )
            ON DUPLICATE KEY UPDATE
                Name = VALUES(Name),
                AsciiName = VALUES(AsciiName),
                GeoNameId = VALUES(GeoNameId);
            """;

		await using var cmd = new MySqlCommand(sql, connection, transaction as MySqlTransaction);

		cmd.Parameters.AddWithValue("@Code", code);
		cmd.Parameters.AddWithValue("@Name", name);
		cmd.Parameters.AddWithValue("@AsciiName", asciiName);
		cmd.Parameters.AddWithValue("@GeoNameId", (object?)geoNameId ?? DBNull.Value);

		await cmd.ExecuteNonQueryAsync();
		imported++;

		if (imported % 1000 == 0)
			Console.WriteLine($"Importeret {imported} admin2 codes...");
	}

	await transaction.CommitAsync();

	Console.WriteLine("Import af admin2 codes færdig.");
	Console.WriteLine($"Importeret: {imported}");
	Console.WriteLine($"Sprunget over: {skipped}");
}

static async Task ImportPlaces(string connectionString, string filePath)
{
	if (!File.Exists(filePath))
	{
		Console.WriteLine($"Filen findes ikke: {filePath}");
		return;
	}

	await using var connection = new MySqlConnection(connectionString);
	await connection.OpenAsync();

	Console.WriteLine("Forbundet til database.");
	Console.WriteLine("Starter import af steder...");

	var imported = 0;
	var skipped = 0;

	await using var transaction = await connection.BeginTransactionAsync();

	foreach (var line in File.ReadLines(filePath))
	{
		if (string.IsNullOrWhiteSpace(line))
			continue;

		var cols = line.Split('\t');

		if (cols.Length < 15)
		{
			skipped++;
			continue;
		}

		var geonameId = int.Parse(cols[0]);
		var name = cols[1];
		var asciiName = cols[2];
		var latitude = double.Parse(cols[4], CultureInfo.InvariantCulture);
		var longitude = double.Parse(cols[5], CultureInfo.InvariantCulture);
		var featureClass = cols[6];
		var featureCode = cols[7];
		var countryCode = cols[8];
		var admin1Code = cols[10];
		var admin2Code = cols[11];

		long.TryParse(cols[14], out var population);

		var sql = """
            INSERT INTO geoname_places
            (
                geoname_id,
                name,
                ascii_name,
                country_code,
                admin1_code,
                latitude,
                longitude,
                population,
                feature_class,
                feature_code,
                admin2_code
            )
            VALUES
            (
                @geoname_id,
                @name,
                @ascii_name,
                @country_code,
                @admin1_code,
                @latitude,
                @longitude,
                @population,
                @feature_class,
                @feature_code,
                @admin2_code
            )
            ON DUPLICATE KEY UPDATE
                name = VALUES(name),
                ascii_name = VALUES(ascii_name),
                country_code = VALUES(country_code),
                admin1_code = VALUES(admin1_code),
                latitude = VALUES(latitude),
                longitude = VALUES(longitude),
                population = VALUES(population),
                feature_class = VALUES(feature_class),
                feature_code = VALUES(feature_code),
                admin2_code = VALUES(admin2_code);
            """;

		await using var cmd = new MySqlCommand(sql, connection, transaction as MySqlTransaction);

		cmd.Parameters.AddWithValue("@geoname_id", geonameId);
		cmd.Parameters.AddWithValue("@name", name);
		cmd.Parameters.AddWithValue("@ascii_name", asciiName);
		cmd.Parameters.AddWithValue("@country_code", countryCode);
		cmd.Parameters.AddWithValue("@admin1_code", admin1Code);
		cmd.Parameters.AddWithValue("@latitude", latitude);
		cmd.Parameters.AddWithValue("@longitude", longitude);
		cmd.Parameters.AddWithValue("@population", population);
		cmd.Parameters.AddWithValue("@feature_class", featureClass);
		cmd.Parameters.AddWithValue("@feature_code", featureCode);
		cmd.Parameters.AddWithValue("@admin2_code", admin2Code);

		await cmd.ExecuteNonQueryAsync();
		imported++;

		if (imported % 1000 == 0)
			Console.WriteLine($"Importeret {imported} steder...");
	}

	await transaction.CommitAsync();

	Console.WriteLine("Import af steder færdig.");
	Console.WriteLine($"Importeret: {imported}");
	Console.WriteLine($"Sprunget over: {skipped}");
}

static async Task ImportAlternateNames(string connectionString, string filePath)
{
	if (!File.Exists(filePath))
	{
		Console.WriteLine($"Filen findes ikke: {filePath}");
		return;
	}

	await using var connection = new MySqlConnection(connectionString);
	await connection.OpenAsync();

	Console.WriteLine("Forbundet til database.");
	Console.WriteLine("Henter eksisterende GeoName IDs...");

	var existingGeoIds = new HashSet<int>();

	await using (var cmd = new MySqlCommand("SELECT geoname_id FROM geoname_places;", connection))
	await using (var reader = await cmd.ExecuteReaderAsync())
	{
		while (await reader.ReadAsync())
			existingGeoIds.Add(reader.GetInt32(0));
	}

	Console.WriteLine($"Fundet {existingGeoIds.Count} steder.");
	Console.WriteLine("Starter import af alternate names...");

	var imported = 0;
	var skipped = 0;

	await using var transaction = await connection.BeginTransactionAsync();

	foreach (var line in File.ReadLines(filePath))
	{
		if (string.IsNullOrWhiteSpace(line))
			continue;

		var cols = line.Split('\t');

		if (cols.Length < 6)
		{
			skipped++;
			continue;
		}

		if (!int.TryParse(cols[0], out var alternateNameId))
		{
			skipped++;
			continue;
		}

		if (!int.TryParse(cols[1], out var geonameId))
		{
			skipped++;
			continue;
		}

		if (!existingGeoIds.Contains(geonameId))
		{
			skipped++;
			continue;
		}

		var isoLanguage = string.IsNullOrWhiteSpace(cols[2]) ? null : cols[2];
		var alternateName = cols[3];

		if (string.IsNullOrWhiteSpace(alternateName))
		{
			skipped++;
			continue;
		}

		var isPreferredName = cols[4] == "1";
		var isShortName = cols[5] == "1";

		var sql = """
            INSERT INTO geo_alternate_names
            (
                GeoAlternateNameId,
                GeoNameId,
                AlternateName,
                IsoLanguage,
                IsPreferredName,
                IsShortName
            )
            VALUES
            (
                @GeoAlternateNameId,
                @GeoNameId,
                @AlternateName,
                @IsoLanguage,
                @IsPreferredName,
                @IsShortName
            )
            ON DUPLICATE KEY UPDATE
                GeoNameId = VALUES(GeoNameId),
                AlternateName = VALUES(AlternateName),
                IsoLanguage = VALUES(IsoLanguage),
                IsPreferredName = VALUES(IsPreferredName),
                IsShortName = VALUES(IsShortName);
            """;

		await using var cmd = new MySqlCommand(sql, connection, transaction as MySqlTransaction);

		cmd.Parameters.AddWithValue("@GeoAlternateNameId", alternateNameId);
		cmd.Parameters.AddWithValue("@GeoNameId", geonameId);
		cmd.Parameters.AddWithValue("@AlternateName", alternateName);
		cmd.Parameters.AddWithValue("@IsoLanguage", (object?)isoLanguage ?? DBNull.Value);
		cmd.Parameters.AddWithValue("@IsPreferredName", isPreferredName);
		cmd.Parameters.AddWithValue("@IsShortName", isShortName);

		await cmd.ExecuteNonQueryAsync();
		imported++;

		if (imported % 5000 == 0)
			Console.WriteLine($"Importeret {imported} alternate names...");
	}

	await transaction.CommitAsync();

	Console.WriteLine("Import af alternate names færdig.");
	Console.WriteLine($"Importeret: {imported}");
	Console.WriteLine($"Sprunget over: {skipped}");
}