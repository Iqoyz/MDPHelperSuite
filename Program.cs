using fyp_server.Services;
using fyp_server.Settings;
using Microsoft.AspNetCore.Http.Features; // For configuring HTTP features like request body limits

var builder = WebApplication.CreateBuilder(args);

// Add CORS (Cross-Origin Resource Sharing) policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", policy =>
    {
        policy.AllowAnyOrigin() // Allow requests from any origin
            .AllowAnyMethod() // Allow all HTTP methods (GET, POST, PUT, etc.)
            .AllowAnyHeader(); // Allow all HTTP headers
        
        // policy.WithOrigins("https://your-domain.com")
        //     .AllowAnyMethod()
        //     .AllowAnyHeader();
    });
});

// Configure form options for file uploads
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 500 * 1024 * 1024; // Set the maximum file size limit for uploads to 500 MB
});

// Configure Kestrel web server options
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 524288000; // Set the maximum request body size to 500 MB
});

// Add services to the container
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<DatabaseSetting>(builder.Configuration.GetSection("DatabaseSetting"));

builder.Services.AddSingleton<FirmwareService>();
builder.Services.AddSingleton<AppInfoService>();

builder.Services.AddControllers()
    .AddJsonOptions(
        options => options.JsonSerializerOptions.PropertyNamingPolicy = null);


var app = builder.Build();

app.UseMiddleware<ApiKeyMiddleware>();

app.UseCors("AllowAllOrigins");

// Serve static files, allowing downloads with unknown file types
app.UseStaticFiles(new StaticFileOptions
{
    ServeUnknownFileTypes = true, // Allow serving files with unknown extensions
    DefaultContentType = "application/octet-stream" // Default MIME type for files
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

app.MapControllers();

app.Run();