using Microsoft.AspNetCore.Mvc;
using Task_Manager_API.Services.OCR;

namespace Task_Manager_API.Controllers.OCR;

[ApiController]
[Route("api/ocr")]
public class OCRController(IOcrService ocrService) : ControllerBase
{
    [HttpPost("mrz-code")]
    public async Task<IActionResult> Post(
        IFormFile back
        )
    {
        try
        {
            var result = await ocrService.BackSideProcessing(back);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}