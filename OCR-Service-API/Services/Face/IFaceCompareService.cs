namespace Task_Manager_API.Services.Face;

public interface IFaceCompareService
{
    Task<float> CompareAsync(IFormFile selfie, IFormFile document);
}