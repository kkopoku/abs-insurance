using Hubtel.Insurance.API.Models;

namespace Hubtel.Insurance.API.Repositories;


public interface IPolicyComponentRepository {
    
    Task<PolicyComponent> CreateAsync(PolicyComponent policyComponent);
    Task<List<PolicyComponent>> GetAllAsync();

}