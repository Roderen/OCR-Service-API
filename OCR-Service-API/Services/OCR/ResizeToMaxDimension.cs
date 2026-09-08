using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace Task_Manager_API.Services.OCR;

public static class ImageProcessor
{
    public static async Task<Stream> ResizeToMaxDimension(IFormFile file, int maxDimension = 1600)
    {
        Image image;
        try
        {
            image = await Image.LoadAsync(file.OpenReadStream());
        }
        catch (Exception ex)
        {
            throw new Exception("Uploaded file is not a valid image", ex);
        }

        using (image)
        {
            if (image.Width > maxDimension || image.Height > maxDimension)
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = new Size(maxDimension, maxDimension)
                }));
            }

            var outputStream = new MemoryStream();
            await image.SaveAsync(outputStream, new JpegEncoder { Quality = 90 });
            outputStream.Position = 0;

            return outputStream;
        }
    }
}