using Amazon;
using Amazon.Rekognition;
using Amazon.Runtime;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Task_Manager_API.Data;
using Task_Manager_API.Services.Face;
using Task_Manager_API.Services.OCR;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient<IOcrService, OcrService>(client =>
{
    var url = builder.Configuration["OcrService:Url"] ?? "http://localhost:8000";
    client.BaseAddress = new Uri(url);
    client.Timeout = TimeSpan.FromSeconds(60);
});

builder.Services.AddScoped<IAmazonRekognition>(sp =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var credentials = new BasicAWSCredentials(cfg["AWS:AccessKey"], cfg["AWS:SecretKey"]);
    var region = RegionEndpoint.GetBySystemName(cfg["AWS:Region"]);
    return new AmazonRekognitionClient(credentials, region);
});

builder.Services.AddScoped<IFaceCompareService, FaceCompareService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); 
}

// app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();