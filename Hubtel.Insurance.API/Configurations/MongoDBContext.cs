using Hubtel.Insurance.API.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Hubtel.Insurance.API.Configurations;

public class MongoDBContext {
    
    private readonly IMongoDatabase _database;
    private readonly ILogger<MongoDBContext> _logger;

    public MongoDBContext(IOptions<MongoDBSettings> mongoDBSettings, ILogger<MongoDBContext> logger){
        var client = new MongoClient(mongoDBSettings.Value.ConnectionString);
        _database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
        _logger = logger;
    }

    public IMongoCollection<Policy> Policies => _database.GetCollection<Policy>("Policies");
    public IMongoCollection<PolicyComponent> PolicyComponents => _database.GetCollection<PolicyComponent>("PolicyComponents");


    public bool TestConnection () {
        string tag = "[MongoDBContext][TestConnection]";
        _logger.LogInformation($"{tag} Attempting to connect to the database ...");
        try{
            _database.ListCollections();
            _logger.LogInformation($"{tag} Connected successfully");
            return true;
        }catch(Exception e){
            _logger.LogInformation($"{tag} Attempt failed. Error {e.Message}");
            return false;

        }
    }

}
