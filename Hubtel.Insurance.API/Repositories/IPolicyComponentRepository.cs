namespace Hubtel.Insurance.API.Repositories;


using Hubtel.Insurance.API.DTOs;
using Hubtel.Insurance.API.Models;

public interface IPolicyComponentRepository {
    
    Task<PolicyComponent> CreateAsync(PolicyComponent policyComponent);
    Task<List<PolicyComponent>> GetAllAsync();
    Task<bool> UpdatePolicyComponentAsync(UpdatePolicyComponentDTO component);

}