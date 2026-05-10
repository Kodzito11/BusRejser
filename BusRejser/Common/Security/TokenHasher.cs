using System.Text;
using System.Security.Cryptography;

namespace BusRejser.Common.Security
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
