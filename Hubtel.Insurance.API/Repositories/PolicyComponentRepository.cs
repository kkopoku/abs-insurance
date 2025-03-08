using Hubtel.Insurance.API.Configurations;
using Hubtel.Insurance.API.Models;
using MongoDB.Driver;

namespace Hubtel.Insurance.API.Repositories;


public class PolicyComponentRepository(
    ILogger<PolicyComponentRepository> logger,
    MongoDBContext mongoDBContext
):IPolicyComponentRepository {

    private readonly ILogger<PolicyComponentRepository> _logger = logger;
    private readonly IMongoCollection<PolicyComponent> _policyComponents = mongoDBContext.PolicyComponents;


    public async Task<PolicyComponent> CreateAsync(PolicyComponent policyComponent){
        await _policyComponents.InsertOneAsync(policyComponent);
        return policyComponent;
    }

    public async Task<List<PolicyComponent>> GetAllAsync(){
        return await _policyComponents.Find(_ => true).ToListAsync();
    }

}