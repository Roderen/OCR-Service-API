using Task_Manager_API.DTO.Document;
using Task_Manager_API.DTO.OCR;
using Task_Manager_API.Models.Document;

namespace Task_Manager_API.Services.OCR;

public interface IOcrService
{
    Task<string> BackSideProcessing(IFormFile image);
}