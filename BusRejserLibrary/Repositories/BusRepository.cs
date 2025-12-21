using System;
using System.Collections.Generic;
using MySqlConnector;
using BusRejserLibrary.Models;

namespace BusRejserLibrary.Repositories
{
	public class BusRepository
	{
		private readonly string _connectionString;

		public BusRepository(string connectionString)
		{
			_connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
		}

		public int Create(Bus bus)
		{
			if (bus == null) throw new ArgumentNullException(nameof(bus));

			const string sql = @"
			INSERT INTO bus (Registreringnummer, Model, Busselskab, Status, Type, Kapasitet)
			VALUES (@reg, @model, @selskab, @status, @type, @kap);
			SELECT LAST_INSERT_ID();";

			using var conn = new MySqlConnection(_connectionString);
			conn.Open();

			using var cmd = new MySqlCommand(sql, conn);
			cmd.Parameters.AddWithValue("@reg", bus.Registreringnummer);
			cmd.Parameters.AddWithValue("@model", bus.Model);
			cmd.Parameters.AddWithValue("@selskab", bus.Busselskab);
			cmd.Parameters.AddWithValue("@status", (int)bus.Status);
			cmd.Parameters.AddWithValue("@type", (int)bus.Type);
			cmd.Parameters.AddWithValue("@kap", bus.Kapasitet);

			var idObj = cmd.ExecuteScalar();
			var newId = Convert.ToInt32(idObj);

			bus.busId = newId;
			return newId;
		}

		public Bus? GetById(int id)
		{
			const string sql = @"
			SELECT busId, Registreringnummer, Model, Busselskab, Status, Type, Kapasitet
			FROM bus
			WHERE busId = @id
			LIMIT 1;";

			using var conn = new MySqlConnection(_connectionString);
			conn.Open();

			using var cmd = new MySqlCommand(sql, conn);
			cmd.Parameters.AddWithValue("@id", id);

			using var reader = cmd.ExecuteReader();
			if (!reader.Read()) return null;

			return MapBus(reader);
		}

		public List<Bus> GetAll()
		{
			const string sql = @"
			SELECT busId, Registreringnummer, Model, Busselskab, Status, Type, Kapasitet
			FROM bus;";

			var list = new List<Bus>();

			using var conn = new MySqlConnection(_connectionString);
			conn.Open();

			using var cmd = new MySqlCommand(sql, conn);
			using var reader = cmd.ExecuteReader();

			while (reader.Read())
				list.Add(MapBus(reader));

			return list;
		}

		public bool Update(Bus bus)
		{
			if (bus == null) throw new ArgumentNullException(nameof(bus));

			const string sql = @"
			UPDATE bus
			SET Registreringnummer = @reg,
				Model = @model,
				Busselskab = @selskab,
				Status = @status,
				Type = @type,
				Kapasitet = @kap
			WHERE busId = @id;";

			using var conn = new MySqlConnection(_connectionString);
			conn.Open();

			using var cmd = new MySqlCommand(sql, conn);
			cmd.Parameters.AddWithValue("@id", bus.busId);
			cmd.Parameters.AddWithValue("@reg", bus.Registreringnummer);
			cmd.Parameters.AddWithValue("@model", bus.Model);
			cmd.Parameters.AddWithValue("@selskab", bus.Busselskab);
			cmd.Parameters.AddWithValue("@status", (int)bus.Status);
			cmd.Parameters.AddWithValue("@type", (int)bus.Type);
			cmd.Parameters.AddWithValue("@kap", bus.Kapasitet);

			return cmd.ExecuteNonQuery() > 0;
		}

		public bool Delete(int id)
		{
			const string sql = @"DELETE FROM bus WHERE busId = @id;";

			using var conn = new MySqlConnection(_connectionString);
			conn.Open();

			using var cmd = new MySqlCommand(sql, conn);
			cmd.Parameters.AddWithValue("@id", id);

			return cmd.ExecuteNonQuery() > 0;
		}

		private static Bus MapBus(MySqlDataReader reader)
		{
			var busId = reader.GetInt32("busId");
			var reg = reader.GetString("Registreringnummer");
			var model = reader.GetString("Model");
			var selskab = reader.IsDBNull(reader.GetOrdinal("Busselskab"))
				? ""
				: reader.GetString("Busselskab");

			var status = (BusStatus)reader.GetInt32("Status");
			var type = (BusType)reader.GetInt32("Type");
			var kap = reader.GetInt32("Kapasitet");

			var bus = Bus.Create(reg, model, selskab, status, type, kap);
			bus.busId = busId;

			// hvis Faceliteter ikke er sat i din Bus.Create -> undgå null senere
			bus.Faceliteter ??= new List<Facilitet>();

			return bus;
		}
	}
}
