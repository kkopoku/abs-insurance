namespace Hubtel.Insurance.API.Repositories;


using Hubtel.Insurance.API.Configurations;
using Hubtel.Insurance.API.Models;
using MongoDB.Driver;
using Hubtel.Insurance.API.DTOs;
using Newtonsoft.Json;
using MongoDB.Bson;

public class PolicyComponentRepository(
    ILogger<PolicyComponentRepository> logger,
    MongoDBContext mongoDBContext
) : IPolicyComponentRepository
{

    private readonly ILogger<PolicyComponentRepository> _logger = logger;
    private readonly IMongoCollection<PolicyComponent> _policyComponents = mongoDBContext.PolicyComponents;


    public async Task<PolicyComponent> CreateAsync(PolicyComponent policyComponent)
    {
        await _policyComponents.InsertOneAsync(policyComponent);
        return policyComponent;
    }

    public async Task<List<PolicyComponent>> GetAllAsync()
    {
        return await _policyComponents.Find(_ => true).ToListAsync();
    }

    public async Task<bool> UpdatePolicyComponentAsync(UpdatePolicyComponentDTO component)
    {
        const string tag = "[PolicyComponentRepository][UpdatePolicyComponentAsync]";
        _logger.LogInformation($"{tag} Start updating policy component: {JsonConvert.SerializeObject(component, Formatting.Indented)}");


        var filter =Builders<PolicyComponent>.Filter.And(
            Builders<PolicyComponent>.Filter.Eq(c => c.PolicyId, component.PolicyId),
            Builders<PolicyComponent>.Filter.Eq(c => c.Sequence, component.Sequence)
        );
        var updateDef = new List<UpdateDefinition<PolicyComponent>>();

        if (component.FlatValue.HasValue){
            updateDef.Add(Builders<PolicyComponent>.Update.Set(c => c.FlatValue, component.FlatValue));
            updateDef.Add(Builders<PolicyComponent>.Update.Set(c => c.Percentage, 0)); // Reset Percentage
            _logger.LogInformation($"{tag} Updating FlatValue to {component.FlatValue} and resetting percentage to 0");
        }

        if (component.PercentageValue.HasValue){
            updateDef.Add(Builders<PolicyComponent>.Update.Set(c => c.Percentage, component.PercentageValue));
            updateDef.Add(Builders<PolicyComponent>.Update.Set(c => c.FlatValue, 0)); // Reset FlatValue
            _logger.LogInformation($"{tag} Updating Percentage to {component.PercentageValue} and resetting flatValue to 0");
        }

        _logger.LogInformation($"{tag} Update Definitions: {JsonConvert.SerializeObject(updateDef, Formatting.Indented)}");

        if (updateDef.Count == 0){
            _logger.LogInformation($"{tag} No fields to update for Sequence: {component.Sequence}");
            return false;
        }

        var update = Builders<PolicyComponent>.Update.Combine(updateDef);

        _logger.LogInformation($"{tag} Updating ... ");
        var result = await _policyComponents.UpdateOneAsync(filter, update);

        if(result.ModifiedCount > 0){
            _logger.LogInformation($"{tag} Update done successfully");
        }

        return result.ModifiedCount > 0;
    }


    public async Task<long> DeletePolicyComponentsByPolicyIdAsync(string policyId){
        const string tag = "[PolicyComponentRepository][DeletePolicyComponentsByPolicyIdAsync]";
        _logger.LogInformation($"{tag} Start deleting policy components with Policy ID: {policyId}");

        var filter = Builders<PolicyComponent>.Filter.Eq(c => c.PolicyId, policyId);
        var result = await _policyComponents.DeleteManyAsync(filter);

        if (result.DeletedCount > 0){
            _logger.LogInformation($"{tag} Successfully deleted {result.DeletedCount} policy components with Policy ID: {policyId}");
            return result.DeletedCount;
        }

        _logger.LogWarning($"{tag} No policy components found with Policy ID: {policyId}");
        return 0;
    }

}