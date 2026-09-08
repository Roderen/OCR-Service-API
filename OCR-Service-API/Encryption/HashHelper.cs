using System.Security.Cryptography;
using System.Text;

namespace Task_Manager_API.Encryption;

public static class HashHelper
{
    public static string Hash(string value)
    {
        using var sha256 = SHA256.Create();

        var bytes = Encoding.UTF8.GetBytes(value);
        var hash = sha256.ComputeHash(bytes);

        return Convert.ToBase64String(hash);
    }
}