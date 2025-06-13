using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace fyp_server.Models;

public class AppInfo
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; } // Unique ID for the entry

    [JsonPropertyName("FileName")] public string FileName { get; set; } = null!; // Author or uploader of the file

    [JsonPropertyName("Type")] public string FileType { get; set; } = null!; // ".exe" or ".pkg"

    [JsonPropertyName("Version")] public string Version { get; set; } = null!; // App version, e.g., "1.0.1"

    [JsonPropertyName("UpdatedDate")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime UpdatedDate { get; set; } = DateTime.UtcNow; // Last updated date

    [JsonPropertyName("DownloadUrl")] public string DownloadUrl { get; set; } = null!; // URL for downloading the file

    [JsonPropertyName("Author")] public string Author { get; set; } = null!; // Author or uploader of the file
}