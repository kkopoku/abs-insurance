namespace Hubtel.Insurance.API.Repositories;

using Hubtel.Insurance.API.Configurations;
using Hubtel.Insurance.API.DTOs;
using Hubtel.Insurance.API.Models;
using MongoDB.Driver;
using Newtonsoft.Json;

public class PolicyRepository
(
    ILogger<PolicyRepository> logger,
    MongoDBContext mongoDBContext
) : IPolicyRepository
{

    private readonly ILogger<PolicyRepository> _logger = logger;
    private readonly IMongoCollection<Policy> _policies = mongoDBContext.Policies;
    private readonly IMongoCollection<PolicyComponent> _policyComponents = mongoDBContext.PolicyComponents;


    public async Task<Policy> CreateAsync(Policy policy)
    {
        await _policies.InsertOneAsync(policy);
        return policy;
    }

    public async Task<List<GetPoliciesDTO>> GetAllAsync(int pageNumber, int pageSize)
    {
        string tag = "[PolicyRepository][GetAllAsync]";
        _logger.LogInformation($"{tag} Fetching policies from database ...");

        try
        {
            var policies = await _policies.Aggregate()
                .Lookup(
                    _policyComponents,   // Collection to join
                    policy => policy.Id, // Local field in Policy
                    component => component.PolicyId, // Foreign field in PolicyComponent
                    (GetPoliciesDTO policy) => policy.Components // Result field in DTO
                )
                .Project(p => new GetPoliciesDTO
                {  // Explicit projection
                    Id = p.Id.ToString(),
                    PolicyName = p.PolicyName,
                    PolicyId = p.PolicyId,
                    Components = p.Components
                })
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            _logger.LogInformation($"{tag} Policies successfully fetched from DB");
            return policies;
        }
        catch (Exception e)
        {
            _logger.LogError($"{tag} Error getting all policies from database. Error: {e.Message}");
            throw new Exception(e.Message);
        }
    }



    public async Task<Policy?> GetByIdAsync(int id)
    {
        const string tag = "[PolicyRepository][GetByIdAsync]";
        var policy = await _policies.Find(p => p.PolicyId == id).FirstOrDefaultAsync();
        _logger.LogInformation($"{tag} This is the policy: {JsonConvert.SerializeObject(policy, Formatting.Indented)}");
        if (policy != null && policy.Components != null)
        {
            policy.Components = await _policyComponents.Find(c => c.PolicyId == policy.Id).ToListAsync();
        }
        return policy;
    }


    public async Task<bool> UpdateAsync(UpdatePolicyDTO updateDto)
    {
        var filter = Builders<Policy>.Filter.Eq(p => p.PolicyId, int.Parse(updateDto.PolicyId));
        var updateDef = new List<UpdateDefinition<Policy>>();

        if (!string.IsNullOrEmpty(updateDto.PolicyName))
        {
            updateDef.Add(Builders<Policy>.Update.Set(p => p.PolicyName, updateDto.PolicyName));
        }

        if (updateDef.Count == 0) return false;

        var update = Builders<Policy>.Update.Combine(updateDef);
        var result = await _policies.UpdateOneAsync(filter, update);

        return result.ModifiedCount > 0;
    }


    public async Task<bool> DeletePolicyByPolicyIdAsync(string id)
    {
        const string tag = "[PolicyRepository][DeletePolicyByPolicyIdAsync]";
        _logger.LogInformation($"{tag} Start deleting policy with PolicyId: {id}");

        var filter = Builders<Policy>.Filter.Eq(p => p.PolicyId, int.Parse(id));
        var result = await _policies.DeleteOneAsync(filter);

        if (result.DeletedCount > 0)
        {
            _logger.LogInformation($"{tag} Successfully deleted policy with PolicyId: {id}");
            return true;
        }

        _logger.LogWarning($"{tag} No policy found with PolicyId: {id}");
        return false;
    }



}