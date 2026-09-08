using System.Security.Cryptography;

namespace Task_Manager_API.Encryption;

public static class Decryptor
{
    private const int KeySize = 256;
    private const int BlockSize = 128;

    private static readonly byte[] Key;

    static Decryptor()
    {
        var base64Key = Environment.GetEnvironmentVariable("ENCRYPTION_KEY");

        if (string.IsNullOrEmpty(base64Key))
        {
            throw new Exception("ENCRYPTION_KEY is missing in environment variables");
        }

        Key = Convert.FromBase64String(base64Key);

        if (Key.Length != 32)
        {
            throw new Exception("ENCRYPTION_KEY must be 32 bytes (256 bits)");
        }
    }

    public static string Decrypt(string encryptedData)
    {
        var combined = Convert.FromBase64String(encryptedData);
        
        var iv = new byte[16];
        var cipherText = new byte[combined.Length - 16];

        Array.Copy(combined, 0, iv, 0, 16);
        Array.Copy(combined, 16, cipherText, 0, cipherText.Length);


        using var aes = Aes.Create();

        aes.KeySize = KeySize;
        aes.BlockSize = BlockSize;

        aes.Key = Key;
        aes.IV = iv;


        using var decryptor = aes.CreateDecryptor();
        using var msDecrypt = new MemoryStream(cipherText);
        using var csDecrypt = new CryptoStream(
            msDecrypt,
            decryptor,
            CryptoStreamMode.Read
        );
        using var srDecrypt = new StreamReader(csDecrypt);

        return srDecrypt.ReadToEnd();
    }
}