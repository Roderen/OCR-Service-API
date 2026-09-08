namespace Task_Manager_API.Encryption;

public class EncryptionResult
{
    public string EncryptedData { get; set; } = string.Empty;


    public static EncryptionResult CreateEncryptedData(
        byte[] data,
        byte[] iv)
    {
        // IV + encrypted data
        var combined = new byte[iv.Length + data.Length];

        Array.Copy(iv, 0, combined, 0, iv.Length);
        Array.Copy(data, 0, combined, iv.Length, data.Length);


        return new EncryptionResult
        {
            EncryptedData = Convert.ToBase64String(combined)
        };
    }


    public (byte[] iv, byte[] encryptedData) GetIVAndEncryptedData()
    {
        var combined = Convert.FromBase64String(EncryptedData);

        var iv = new byte[16];
        var encryptedData = new byte[combined.Length - 16];


        Array.Copy(combined, 0, iv, 0, 16);
        Array.Copy(combined, 16, encryptedData, 0, encryptedData.Length);


        return (iv, encryptedData);
    }
}