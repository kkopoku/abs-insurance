namespace Hubtel.Insurance.API.Services;

using Hubtel.Insurance.API.DTOs;
using Hubtel.Insurance.API.Models;

public interface IPolicyService {
    Task<ApiResponse<PaginatedResponse<GetPoliciesDTO>>> GetAllPolicies(int pageNumber, int pageSize);
    Task<ApiResponse<Policy>> GetPolicyDetailsAsync(int id);
    Task<ApiResponse<CalculatePremiumDTO>> CalculatePremium(RequestQuoteDTO requestQuoteDTO);
    Task<ApiResponse<object>> CreatePolicyAsync(CreatePolicyDTO createPolicyDTO);
    Task<ApiResponse<Policy>> UpdatePolicyAsync(UpdatePolicyDTO updatePolicyDTO);
    Task<ApiResponse<string>> DeletePolicyByPolicyId(string policyId);
}