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



	}
}
