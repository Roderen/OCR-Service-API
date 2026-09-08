using Microsoft.AspNetCore.Mvc;
using Task_Manager_API.Services.Face;

namespace Task_Manager_API.Controllers.Face;

[ApiController]
[Route("api/face")]
public class FaceController(IFaceCompareService faceService) : ControllerBase
{
    [HttpPost("compare")]
    public async Task<IActionResult> Compare(IFormFile selfie, IFormFile front)
    {
        var similarity = await faceService.CompareAsync(selfie, front);
        return Ok(new { similarity, match = similarity >= 80 });
    }
}