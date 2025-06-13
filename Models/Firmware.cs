using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace fyp_server.Models;

public class Firmware
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("FirmwareName")]
    [JsonPropertyName("Name")]
    public string FirmwareName { get; set; } = null!;

    public string Author { get; set; } = null!;

    [BsonElement("FileData")]
    [JsonPropertyName("Data")]
    public byte[] FileData { get; set; } = [];
}