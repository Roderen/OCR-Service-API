using Amazon.Rekognition;
using Amazon.Rekognition.Model;

namespace Task_Manager_API.Services.Face;

public class FaceCompareService(IAmazonRekognition rekognition) : IFaceCompareService
{
    public async Task<float> CompareAsync(IFormFile selfie, IFormFile document)
    {
        using var selfieStream = new MemoryStream();
        await selfie.CopyToAsync(selfieStream);

        using var docStream = new MemoryStream();
        await document.CopyToAsync(docStream);

        var response = await rekognition.CompareFacesAsync(new CompareFacesRequest
        {
            SourceImage = new Image { Bytes = selfieStream },
            TargetImage = new Image { Bytes = docStream },
            SimilarityThreshold = 80F
        });

        return (float)(response.FaceMatches.Count > 0
            ? response.FaceMatches[0].Similarity
            : 0f)!;
    }
}