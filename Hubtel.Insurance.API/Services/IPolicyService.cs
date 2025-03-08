namespace Hubtel.Insurance.API.Services;

using Hubtel.Insurance.API.DTOs;
using Hubtel.Insurance.API.Models;

public interface IPolicyService {
    Task<ApiResponse<Policy>> GetPolicyDetailsAsync(int id);
    Task<ApiResponse<object>> CalculatePremium(RequestQuoteDTO requestQuoteDTO);
}