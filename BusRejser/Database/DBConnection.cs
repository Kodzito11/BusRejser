using MySqlConnector; //skal bruge den her?

namespace BusRejser.Database
{
	public class DBConnection
	{
		private readonly string _connectionString;

		public DBConnection(string connectionString)
		{
			_connectionString = connectionString;
		}

		public MySqlConnection GetConnection()
		{
			return new MySqlConnection(_connectionString);
		}
	}
}
