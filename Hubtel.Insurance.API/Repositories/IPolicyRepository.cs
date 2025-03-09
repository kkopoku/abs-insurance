using Hubtel.Insurance.API.DTOs;
using Hubtel.Insurance.API.Models;

namespace Hubtel.Insurance.API.Repositories;


public interface IPolicyRepository {

    Task<Policy> CreateAsync(Policy policy);
    Task<Policy?> GetByIdAsync(int id);
    Task<List<GetPoliciesDTO>> GetAllAsync(int pageNumber, int pageSize);
    
}