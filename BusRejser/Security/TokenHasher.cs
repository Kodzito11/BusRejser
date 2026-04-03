using System.Text;
using System.Security.Cryptography;
using System.Text;

namespace BusRejser.Security
{
	public static class TokenHasher
	{
		public static string Hash(string input)
		{
			using var sha = SHA256.Create();
			var bytes = Encoding.UTF8.GetBytes(input);
			var hash = sha.ComputeHash(bytes);
			return Convert.ToBase64String(hash);
		}
	}
}
