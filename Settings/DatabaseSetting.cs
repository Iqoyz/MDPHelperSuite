namespace fyp_server.Settings;

public class DatabaseSetting : IDatabaseSetting
{
    // Add collection names as individual properties
    public string FirmwareCollectionName { get; set; } = null!;

    public string AppInfoCollectionName { get; set; } = null!;
    public string ConnectionString { get; set; } = null!; // MongoDB connection string
    public string DatabaseName { get; set; } = null!; // Name of the MongoDB database
}