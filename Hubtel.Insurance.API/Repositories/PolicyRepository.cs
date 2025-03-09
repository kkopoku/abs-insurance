namespace Hubtel.Insurance.API.Repositories;

using Hubtel.Insurance.API.Configurations;
using Hubtel.Insurance.API.DTOs;
using Hubtel.Insurance.API.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;

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
        var policy = await _policies.Find(p => p.PolicyId == id).FirstOrDefaultAsync();
        if (policy != null)
        {
            policy.Components = await _policyComponents.Find(c => c.PolicyId == policy.Id).ToListAsync();
        }
        return policy;
    }

}