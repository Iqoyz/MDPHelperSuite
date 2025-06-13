namespace fyp_MDPHelperApp.Services.Api;

public class AppinfoApiClient
{
    public static async Task<AppInfoResponse> GetLatestAppInfoAsync(string query)
    {
        var endpoint = $"api/appinfo/latest?{query}";

        // Use the static utility method to send the GET request
        return await ApiClientUtility.GetAsync<AppInfoResponse>(endpoint);
    }
}

// Helper class to match the API response structure
public class AppInfoResponse
{
    public string Id { get; set; }
    public string Type { get; set; }
    public string Version { get; set; }
    public DateTime UpdatedDate { get; set; }
    public string DownloadUrl { get; set; }
    public string Author { get; set; }
}