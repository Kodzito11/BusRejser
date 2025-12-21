using BusRejser.Library.Modeller;
using BusRejser.Database;
using MySql.Data.MySqlClient;

namespace BusRejser.Library.Repositories
{
	public class BusRepository
	{
		private readonly DBConnection _dbConnection;

		public BusRepository(DBConnection dbConnection)
		{
			_dbConnection = dbConnection;
		}

		public List<Bus> GetAll()
		{
			var buses = new List<Bus>();
			var conn = _dbConnection.GetConnection();

			var cmd = new MySqlCommand(
				@"SELECT
					busId,
					Registreringnummer,
					Model,
					Busselskab,
					Status,
					Type
					Kapasitet
				  FROM Busser", conn);

			try
			{
				conn.Open();
				var reader = cmd.ExecuteReader();

				while (reader.Read())
				{
					var bus = Bus.Create(
						reader.GetString(1),
						reader.GetString(2),
						reader.GetString(3),
						(BusStatus)reader.GetInt32(4),
						(BusType)reader.GetInt32(5),
						reader.GetInt32(6)

					);

					bus.busId = reader.GetInt32(0);

					buses.Add(bus);
				}

			}

				finally
			{
				conn.Close();
			}

			return buses;
		}

		public Bus? GetById(int id)
		{
			var conn = _dbConnection.GetConnection();
			var cmd = new MySqlCommand(
				@"SELCT
					busId,
					Registreringnummer,
					Model
					Busselskab
					Status
					Type
					Kapasitet
				From Busser
				WHERE BusId = @id", conn);
			cmd.Parameters.AddWithValue("id", id);


			try
			{

				conn.Open();
				var reader = cmd.ExecuteReader();

				if (reader.Read())
				{
					var bus = Bus.Create(
						reader.GetString(1),
						reader.GetString(2),
						reader.GetString(3),
						(BusStatus)reader.GetInt32(4),
						(BusType)reader.GetInt32(5),
						reader.GetInt32(6)
						);

					bus.busId = reader.GetInt32(0);
					return bus;
				}
			}
			finally
			{
				conn.Close();
			}

			return null;
		}

		public void Add(Bus bus)
		{
			var conn = _dbConnection.GetConnection();
			var cmd = new MySqlCommand(
				@"INSERT INTO Busser
					(Registreingnummer, Model, Busselskab, Status, Type, Kapasitet)
				  VALUES
					(@reg, @model, @selskab, @status, @type, @kap", conn);

			cmd.Parameters.AddWithValue("@Reg", bus.Registreringnummer);
			cmd.Parameters.AddWithValue("@model", bus.Model);
			cmd.Parameters.AddWithValue("@selskab", bus.Busselskab);
			cmd.Parameters.AddWithValue("@type", (int)bus.Status);
			cmd.Parameters.AddWithValue("@kap,", bus.Kapasitet);

			try
			{
				conn.Open();
				cmd.ExecuteNonQuery();
				bus.busId = (int)cmd.LastInsertedId;
			}
			finally
			{
				conn.Close();
			}
		}


		public void Update(Bus bus)
		{

			var conn = _dbConnection.GetConnection();
			var cmd = new MySqlCommand(
				@"UPDATE Busser SET
					Registreingnummer = @reg,
					Model = @model,
					Busselskab = @selskab,
					Status = @status
					Type = @type,
					Kapasitet = @kap
				  WHERE busId = @id", conn);

			cmd.Parameters.AddWithValue("@id", bus.busId);
			cmd.Parameters.AddWithValue("@reg", bus.Registreringnummer);
			cmd.Parameters.AddWithValue("@model", bus.Model);
			cmd.Parameters.AddWithValue("@selskab", bus.Busselskab);
			cmd.Parameters.AddWithValue("@type", (int)bus.Type);
			cmd.Parameters.AddWithValue("@kap", bus.Kapasitet);

			try
			{
				conn.Open();
				cmd.ExecuteNonQuery();


			}
			finally
			{
				conn.Close();
			}
		}
	}
}
