namespace fyp_server.Services;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private const string ApiKeyHeaderName = "X-Api-Key";
    private readonly string _apiKey;

    public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _apiKey = configuration.GetValue<string>("ApiSettings:ApiKey") 
                  ?? throw new InvalidOperationException("API Key not found in configuration.");
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip API key check for Swagger endpoints
        var path = context.Request.Path.Value;
        if (path != null && (path.StartsWith("/swagger") || path.StartsWith("/favicon.ico")))
        {
            await _next(context);
            return;
        }
        
        if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
        {
            context.Response.StatusCode = 401; // Unauthorized
            await context.Response.WriteAsync("API Key was not provided.");
            return;
        }

        if (!string.Equals(extractedApiKey, _apiKey))
        {
            context.Response.StatusCode = 403; // Forbidden
            await context.Response.WriteAsync("Invalid API Key.");
            return;
        }

        await _next(context);
    }
}
