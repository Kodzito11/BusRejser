using System;
using System.Collections.Generic;
using BusRejserLibrary.Database;
using BusRejserLibrary.Models;
using MySqlConnector;

namespace BusRejserLibrary.Repositories
{
	public class FacilitetRepository
	{
		private readonly DBConnection _db;

		public FacilitetRepository(DBConnection db)
		{
			_db = db ?? throw new ArgumentNullException(nameof(db));
		}

		public int Create(Facilitet facilitet)
		{
			if (facilitet == null) throw new ArgumentNullException(nameof(facilitet));

			const string sql = @"
			INSERT INTO facilitet (Name, Description, ExtraPrice, IsActive, Type)
			VALUES (@name, @desc, @price, @active, @type);
			SELECT LAST_INSERT_ID();";

			using var conn = _db.GetConnection();
			conn.Open();

			using var cmd = new MySqlCommand(sql, conn);
			cmd.Parameters.AddWithValue("@name", facilitet.Name);
			cmd.Parameters.AddWithValue("@desc", facilitet.Description);
			cmd.Parameters.AddWithValue("@price", facilitet.ExtraPrice);
			cmd.Parameters.AddWithValue("@active", facilitet.IsActive ? 1 : 0);
			cmd.Parameters.AddWithValue("@type", (int)facilitet.Type);

			var idObj = cmd.ExecuteScalar();
			return Convert.ToInt32(idObj);
		}

		public Facilitet? GetById(int id)
		{
			const string sql = @"
			SELECT Id, Name, Description, ExtraPrice, IsActive, Type
			FROM facilitet
			WHERE Id = @id
			LIMIT 1;";

			using var conn = _db.GetConnection();
			conn.Open();

			using var cmd = new MySqlCommand(sql, conn);
			cmd.Parameters.AddWithValue("@id", id);

			using var reader = cmd.ExecuteReader();
			if (!reader.Read()) return null;

			return MapFacilitet(reader);
		}

		public List<Facilitet> GetAll()
		{
			const string sql = @"
			SELECT Id, Name, Description, ExtraPrice, IsActive, Type
			FROM facilitet;";

			var list = new List<Facilitet>();

			using var conn = _db.GetConnection();
			conn.Open();

			using var cmd = new MySqlCommand(sql, conn);
			using var reader = cmd.ExecuteReader();

			while (reader.Read())
				list.Add(MapFacilitet(reader));

			return list;
		}

		public bool Delete(int id)
		{
			const string sql = @"DELETE FROM facilitet WHERE Id = @id;";

			using var conn = _db.GetConnection();
			conn.Open();

			using var cmd = new MySqlCommand(sql, conn);
			cmd.Parameters.AddWithValue("@id", id);

			return cmd.ExecuteNonQuery() > 0;
		}

		private static Facilitet MapFacilitet(MySqlDataReader reader)
		{
			var id = reader.GetInt32("Id");
			var name = reader.GetString("Name");
			var desc = reader.IsDBNull(reader.GetOrdinal("Description")) ? "" : reader.GetString("Description");
			var price = reader.GetDecimal("ExtraPrice");
			var isActive = reader.GetInt32("IsActive") == 1;
			var type = (FacilitetType)reader.GetInt32("Type");

			// create (sætter ikke Id i din model - den har private set)
			// så vi returnerer objektet uden Id (til API er det fint i første version)
			return Facilitet.Create(name, desc, price, type, isActive);
		}
	}
}
