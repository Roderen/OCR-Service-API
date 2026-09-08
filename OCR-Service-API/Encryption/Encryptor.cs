using System.Security.Cryptography;
using System.Text;

namespace Task_Manager_API.Encryption;

public static class Encryptor
{
    private const int KeySize = 256;
    private const int BlockSize = 128;

    private static readonly byte[] Key;

    static Encryptor()
    {
        var base64Key = Environment.GetEnvironmentVariable("ENCRYPTION_KEY");

        if (string.IsNullOrEmpty(base64Key))
        {
            throw new Exception("ENCRYPTION_KEY is missing in environment variables");
        }

        Key = Convert.FromBase64String(base64Key);

        if (Key.Length != 32)
        {
            throw new Exception("ENCRYPTION_KEY must be 256 bit (32 bytes)");
        }
    }

    public static EncryptionResult Encrypt(string plainText)
    {
        using var aes = Aes.Create();

        aes.KeySize = KeySize;
        aes.BlockSize = BlockSize;

        aes.Key = Key;
        aes.GenerateIV();

        byte[] encryptedData;

        using (var encryptor = aes.CreateEncryptor())
        using (var msEncrypt = new MemoryStream())
        {
            using (var csEncrypt = new CryptoStream(
                       msEncrypt,
                       encryptor,
                       CryptoStreamMode.Write))
            using (var swEncrypt = new StreamWriter(csEncrypt))
            {
                swEncrypt.Write(plainText);
            }
            
            encryptedData = msEncrypt.ToArray();
        }

        return EncryptionResult.CreateEncryptedData(
            encryptedData,
            aes.IV
        );
    }
    
    public static string ComputeLookupHash(string plainText)
    {
        using var hmac = new HMACSHA256(Key);
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(plainText));
        return Convert.ToBase64String(hash);
    }
}