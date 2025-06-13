namespace fyp_MDPHelperApp.Services.Api;

public class FirmwareApiClient
{
    public static async Task<Stream> GetFirmwareFileByNameAsync(string firmwareName)
    {
        var endpoint = $"api/firmware/name/{firmwareName}";

        // Use the static utility method to send the GET request
        var firmwareResponse = await ApiClientUtility.GetAsync<FirmwareResponse>(endpoint);

        if (firmwareResponse.Data == null) throw new Exception($"No firmware found with name '{firmwareName}'.");

        // Decode the base64-encoded file data
        var decodedBytes = Convert.FromBase64String(firmwareResponse.Data);

        return new MemoryStream(decodedBytes);
    }
}

// Class to deserialize the JSON response
public class FirmwareResponse
{
    public string Data { get; set; }
}