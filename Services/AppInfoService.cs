using fyp_server.Models;
using fyp_server.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace fyp_server.Services;

public class AppInfoService
{
    private readonly IMongoCollection<AppInfo> _appInfoCollection;

    public AppInfoService(IOptions<DatabaseSetting> databaseSetting)
    {
        var mongoClient = new MongoClient(databaseSetting.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(databaseSetting.Value.DatabaseName);

        _appInfoCollection = mongoDatabase.GetCollection<AppInfo>(
            databaseSetting.Value.AppInfoCollectionName);

        // Create a unique index on Version and FileType (composite key)
        var indexOptions = new CreateIndexOptions { Unique = true };
        var indexKeys = Builders<AppInfo>.IndexKeys
            .Ascending(a => a.Version)
            .Ascending(a => a.FileType);
        var indexModel = new CreateIndexModel<AppInfo>(indexKeys, indexOptions);

        _appInfoCollection.Indexes.CreateOne(indexModel);
    }


    public async Task<string> UploadFileAsync(IFormFile file, string version, string author, HttpRequest request)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("No file was uploaded.");

        if (string.IsNullOrEmpty(version) || string.IsNullOrEmpty(author))
            throw new ArgumentException("Version and author must be provided.");

        // Define the path to save the file
        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var filePath = Path.Combine(uploadsFolder, file.FileName);

        // Save the file
        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        // Generate the full download URL
        var downloadUrl = $"{request.Scheme}://{request.Host}/uploads/{file.FileName}";

        // Determine file type and handle missing extension
        var fileType = Path.GetExtension(file.FileName);
        if (string.IsNullOrEmpty(fileType)) fileType = ".app"; // Default to ".app" if no extension is provided

        // Save file metadata to the AppInfo collection
        var appInfo = new AppInfo
        {
            FileName = Path.GetFileNameWithoutExtension(file.FileName), // Extract the file name without the extension
            FileType = fileType, // Use the determined file type
            Version = version,
            Author = author,
            DownloadUrl = downloadUrl,
            UpdatedDate = DateTime.UtcNow
        };

        await CreateAsync(appInfo);

        // Return the full download URL
        return downloadUrl;
    }

    // Create: Insert a new AppInfo entry
    public async Task<AppInfo> CreateAsync(AppInfo appInfo)
    {
        // Ensure version and download URL are provided
        if (string.IsNullOrEmpty(appInfo.Version) || string.IsNullOrEmpty(appInfo.DownloadUrl))
            throw new ArgumentException("Version and DownloadUrl cannot be null or empty.");

        await _appInfoCollection.InsertOneAsync(appInfo);
        return appInfo;
    }

    // Read: Get all AppInfo entries
    public async Task<List<AppInfo>> GetAsync()
    {
        return await _appInfoCollection.Find(_ => true).ToListAsync();
    }


    // Read: Get the latest AppInfo by file type
    public async Task<AppInfo?> GetLatestByFileTypeAsync(string fileType)
    {
        return await _appInfoCollection
            .Find(a => a.FileType == fileType)
            .SortByDescending(a => a.UpdatedDate)
            .FirstOrDefaultAsync();
    }

    // Read: Get the latest AppInfo by filename and latest version
    public async Task<AppInfo?> GetLatestByFileNameAndTypeAsync(string fileName, string? fileType = null)
    {
        var filter = Builders<AppInfo>.Filter.Regex("FileName", new BsonRegularExpression(fileName, "i")); // Case-insensitive match for fileName

        // Add the fileType filter if it is provided
        if (!string.IsNullOrEmpty(fileType))
        {
            var typeFilter = Builders<AppInfo>.Filter.Eq("FileType", fileType);
            filter = Builders<AppInfo>.Filter.And(filter, typeFilter);
        }

        return await _appInfoCollection
            .Find(filter)
            .SortByDescending(a => a.Version) // Assuming Version is a comparable property
            .FirstOrDefaultAsync();
    }



    // Read: Get a specific AppInfo by version
    public async Task<AppInfo?> GetByVersionAsync(string version)
    {
        return await _appInfoCollection.Find(a => a.Version == version).FirstOrDefaultAsync();
    }

    // Read: Get an AppInfo entry by version and file type
    public async Task<AppInfo?> GetByVersionAndFileTypeAsync(string version, string fileType)
    {
        return await _appInfoCollection
            .Find(a => a.Version == version && a.FileType == fileType)
            .FirstOrDefaultAsync();
    }

// Delete: Remove an AppInfo entry by version and file type
    public async Task RemoveByVersionAndFileTypeAsync(string version, string fileType)
    {
        await _appInfoCollection
            .DeleteOneAsync(a => a.Version == version && a.FileType == fileType);
    }


    // Delete: Remove all AppInfo entries
    public async Task RemoveAllAsync()
    {
        await _appInfoCollection.DeleteManyAsync(Builders<AppInfo>.Filter.Empty);
    }
}