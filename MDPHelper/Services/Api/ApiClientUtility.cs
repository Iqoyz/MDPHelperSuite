using System.Net.Http.Headers;
using System.Text.Json;

namespace fyp_MDPHelperApp.Services.Api;

public class ApiClientUtility
{
    private const string BASE_URL = "http://localhost:5000";
    // public static readonly string BASE_URL = "http://192.168.50.170:5000";

    private const string API_KEY_HEADER_NAME = "X-Api-Key";
    private const string API_KEY = "iiOHs5Nt-GKgEoWPmtY9Bk-iiSKM-7dSE0LxIvIyo0Q";
        
    private static readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(20) // Set a default timeout
    };

    public static async Task<T> GetAsync<T>(string endpoint)
    {
        var apiUrl = $"{BASE_URL.TrimEnd('/')}/{endpoint.TrimStart('/')}";

        // Create the GET request
        var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        request.Headers.Add(API_KEY_HEADER_NAME, API_KEY);
        
        // Send the request
        var response = await _httpClient.SendAsync(request);

        // Ensure the response is successful
        if (!response.IsSuccessStatusCode)
            throw new Exception($"Request to {apiUrl} failed with status code {response.StatusCode}.");

        // Read and deserialize the response content
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<T>(responseContent);

        if (result == null) throw new Exception($"Failed to deserialize response from {apiUrl}.");

        return result;
    }
    
    public static async Task DownloadFileUsingUrlAsync(string fileUrl, string filePath)
{
    try
    {
        // Create the GET request
        var request = new HttpRequestMessage(HttpMethod.Get, fileUrl);
        request.Headers.Add(API_KEY_HEADER_NAME, API_KEY); // Add API key if required

        // Send the request
        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

        // Ensure the response is successful
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"File download failed. HTTP status code: {response.StatusCode}. Reason: {response.ReasonPhrase}"
            );
        }

        // Ensure the directory exists
        var directoryPath = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        // Delete the file if it already exists
        if (File.Exists(filePath)) File.Delete(filePath);

        // Stream the file to disk
        await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        await using var responseStream = await response.Content.ReadAsStreamAsync();
        await responseStream.CopyToAsync(fileStream);

        Console.WriteLine($"File downloaded successfully to: {filePath}");
    }
    catch (HttpRequestException ex)
    {
        Console.WriteLine($"HTTP error: {ex.Message}");
        ToastMessageHandler.ShowToastAsync($"HTTP error: {ex.Message}");
    }
    catch (UnauthorizedAccessException ex)
    {
        Console.WriteLine($"Access error: {ex.Message}");
        ToastMessageHandler.ShowToastAsync($"Access error: {ex.Message}. Ensure the app has permissions for the path.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Unexpected error: {ex.Message}");
        ToastMessageHandler.ShowToastAsync($"Unexpected error: {ex.Message}");
    }
}

}