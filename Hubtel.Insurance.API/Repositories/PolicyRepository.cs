namespace Hubtel.Insurance.API.Repositories;

using Hubtel.Insurance.API.Configurations;
using Hubtel.Insurance.API.Models;
using MongoDB.Driver;

public class PolicyRepository
(
    ILogger<PolicyRepository> logger,
    MongoDBContext mongoDBContext
) : IPolicyRepository
{

    private readonly ILogger<PolicyRepository> _logger = logger;
    private readonly IMongoCollection<Policy> _policies = mongoDBContext.Policies;


    public async Task<Policy> CreateAsync(Policy policy){
        await _policies.InsertOneAsync(policy);
        return policy;
    }

    public async Task<List<Policy>> GetAllAsync(){
        return await _policies.Find(_ => true).ToListAsync();
    }


    public async Task<Policy> GetByIdAsync(int id){
        return await _policies.Find(p => p.PolicyId == id).FirstOrDefaultAsync();
    }

}