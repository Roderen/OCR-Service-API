using System.Globalization;
using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using MRZCodeParser;
using Task_Manager_API.Data;
using Task_Manager_API.DTO.OCR;
using Task_Manager_API.Encryption;
using Task_Manager_API.Models.Document;

namespace Task_Manager_API.Services.OCR;

public class OcrService(HttpClient httpClient, IConfiguration config, ApplicationDbContext context): IOcrService
{
    public async Task<string> BackSideProcessing(IFormFile file)
    {
        using var form = new MultipartFormDataContent();
        
        // Resize image to max dimension
        await using var resizedStream = await ImageProcessor.ResizeToMaxDimension(file);
        
        using var content = new StreamContent(resizedStream);
        content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        form.Add(content, "file", "document.jpg");

        // Check internal token for OCR service access
        var token = config["OcrService:InternalToken"];
        if (!string.IsNullOrEmpty(token))
            form.Headers.Add("X-Internal-Token", token);
        
        // Forward the image to the python FastAPI OCR microservice
        var response = await httpClient.PostAsync("/ocr", form);

        if (!response.IsSuccessStatusCode)
            throw new Exception($"OCR service returned {response.StatusCode}");

        var result = await response.Content.ReadFromJsonAsync<OcrResultDto>();

        if (result is null)
            throw new Exception("OCR service returned empty response");

        // Filter and clean lines that belong to the MRZ
        var mrzLines = result.Items
            .Where(x => x.Text.Contains('<'))
            .Select(x => x.Text.Replace(" ", "").ToUpper())
            .ToArray();

        // Combine lines back into a single text block separated by newlines
        var resultString = string.Join("\n", mrzLines);

        // Parse raw MRZ text into a structured object
        var code = MrzCode.Parse(resultString);

        // Validate the document expiry date
        if (
            DateOnly.ParseExact(code[FieldType.ExpiryDate], "yyMMdd", CultureInfo.InvariantCulture) < DateOnly.FromDateTime(DateTime.Today)
            )
        {
            throw new Exception("Document has expired");
        }

        // Automatically detect the specific MRZ format (e.g., TD1, TD2, TD3/Passport)
        var format = MrzFormatDetector.DetectFormat(mrzLines);

        if (format == MrzFormat.Unknown)
            throw new Exception($"Unsupported MRZ format. Raw MRZ: {resultString}");

        // Extract the unique document number
        var documentNumber = MrzFormatDetector.ExtractDocumentNumber(format, mrzLines);

        if (string.IsNullOrWhiteSpace(documentNumber))
            throw new Exception("Document number could not be extracted");
        
        // Validate the document number checksum
        string documentNumberCheckDigit;
        try
        {
            documentNumberCheckDigit = code[FieldType.DocumentNumberCheckDigit];
        }
        catch
        {
            throw new Exception("Document number check digit not found in MRZ");
        }

        if (!MrzChecksumValidator.ValidateField(documentNumber, documentNumberCheckDigit[0]))
            throw new Exception($"Invalid document number checksum. Document may be fraudulent or corrupted. Raw MRZ: {resultString}");

        var lookupHash = Encryptor.ComputeLookupHash(documentNumber);

        var existing = await context.Documents
            .FirstOrDefaultAsync(x => x.DocumentIdHash == lookupHash);

        if (existing is null)
        {
            var encryptedDocumentNumber = Encryptor.Encrypt(documentNumber);

            string name;
            try
            {
                name = code[FieldType.Names].Replace(",", "");
            }
            catch
            {
                name = code[FieldType.PrimaryIdentifier].Replace(",", "");
            }

            var parseResult = new DocumentModel
            {
                Name = name,

                BirthDate = DateOnly.ParseExact(
                    code[FieldType.BirthDate],
                    "yyMMdd",
                    CultureInfo.InvariantCulture
                ),

                ExpiryDate = DateOnly.ParseExact(
                    code[FieldType.ExpiryDate],
                    "yyMMdd",
                    CultureInfo.InvariantCulture
                ),

                DocumentId = encryptedDocumentNumber.EncryptedData,
                DocumentIdHash = lookupHash
            };

            context.Documents.Add(parseResult);
            await context.SaveChangesAsync();
        }

        return "Document number already exists";
    }
}