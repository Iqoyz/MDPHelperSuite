using fyp_server.Models;
using fyp_server.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace fyp_server.Services;

public class FirmwareService
{
    private readonly IMongoCollection<Firmware> _firmwareCollection;

    public FirmwareService(IOptions<DatabaseSetting> databaseSetting)
    {
        var mongoClient = new MongoClient(databaseSetting.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(databaseSetting.Value.DatabaseName);

        _firmwareCollection = mongoDatabase.GetCollection<Firmware>(
            databaseSetting.Value.FirmwareCollectionName);

        // Create a unique index on the FirmwareName field
        var indexOptions = new CreateIndexOptions { Unique = true };
        var indexKeys = Builders<Firmware>.IndexKeys.Ascending(f => f.FirmwareName);
        var indexModel = new CreateIndexModel<Firmware>(indexKeys, indexOptions);

        _firmwareCollection.Indexes.CreateOne(indexModel);
    }


    // Create: Insert a new firmware
    public async Task<Firmware> CreateAsync(Firmware firmware)
    {
        // Ensure the firmware name is always lowercase
        firmware.FirmwareName = firmware.FirmwareName.ToLowerInvariant();
        await _firmwareCollection.InsertOneAsync(firmware);
        return firmware;
    }

    // Read: Get all firmwares
    public async Task<List<Firmware>> GetAsync()
    {
        return await _firmwareCollection.Find(_ => true).ToListAsync();
    }

    // Read: Get a specific firmware by name
    public async Task<Firmware?> GetByNameAsync(string firmwareName)
    {
        return await _firmwareCollection.Find(x => x.FirmwareName == firmwareName).FirstOrDefaultAsync();
    }

    // Delete: Remove a firmware by name
    public async Task RemoveByNameAsync(string firmwareName)
    {
        await _firmwareCollection.DeleteOneAsync(x => x.FirmwareName == firmwareName);
    }

    public async Task RemoveAllAsync()
    {
        await _firmwareCollection.DeleteManyAsync(Builders<Firmware>.Filter.Empty);
    }
}