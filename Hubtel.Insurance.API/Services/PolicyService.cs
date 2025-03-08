namespace Hubtel.Insurance.API.Services;

using Hubtel.Insurance.API.Repositories;
using Hubtel.Insurance.API.DTOs;
using Hubtel.Insurance.API.Models;

public class PolicyService(
    IPolicyRepository policyRepository,
    ILogger<PolicyService> logger
):IPolicyService{

    private readonly IPolicyRepository _policyRepository = policyRepository;
    private readonly ILogger<PolicyService> _logger = logger;
    public async Task<ApiResponse<Policy>> GetPolicyDetailsAsync(int id){
        var tag = "[PolicyService][GetPolicyDetailsAsync]";

        _logger.LogInformation($"{tag} Searching for policy with id: {id}");
        var foundPolicy = await _policyRepository.GetByIdAsync(id);
        if(foundPolicy == null){
            _logger.LogInformation($"{tag} Policy with id: {id} not found, returning not found response");
            var notFoundMessage = "Policy not found";
            var response = new ApiResponse<Policy>("404", notFoundMessage);
            return response;
        }
        _logger.LogInformation($"{tag} Policy with id: {id} found, returning success response");
        var successMessage = "Policy fetched successfully";
        return new ApiResponse<Policy>("200", successMessage, foundPolicy);
    }


    public async Task<ApiResponse<object>> CalculatePremium(RequestQuoteDTO requestQuoteDTO){
         var tag = "[PolicyService][CalculatePremium]";
         _logger.LogInformation($"{tag} Calculating premium ...");

        //  Find the policy
        var policyId = requestQuoteDTO.PolicyId;

        var foundPolicy = await _policyRepository.GetByIdAsync(policyId);

        if (foundPolicy == null){
            _logger.LogInformation($"{tag} Policy not found");
            var notFoundMessage = "Policy not found, cannot request quote";
            return new ApiResponse<object>("404", notFoundMessage);
        }

        // Check if all 4 components are present
        var componentCount = foundPolicy.Components.Count;
        if (componentCount != 4){
            _logger.LogInformation($"{tag} Policy components count is invalid. Count == {componentCount}");
            var errorMessage = "Cannot calculate quote for this policy";
            return new ApiResponse<object>("404", errorMessage);
        }

        double totalPremium = 0;
        foreach (var component in foundPolicy.Components.OrderBy(c => c.Sequence)){

            // check if component has both decimal and percentage
            if(component.FlatValue != 0 && component.Percentage != 0){
                _logger.LogInformation($"{tag} Cannot have both flat value and percentage present. flatValue = {component.FlatValue} , percentage = {component.Percentage}");
                var errorMessage = "Something went wrong, please contact support";
                return new ApiResponse<object>("400", errorMessage);
            }

            double componentValue = component.FlatValue + (requestQuoteDTO.MarketValue * component.Percentage / 100);
            if (component.Operation.ToLower() == "add"){
                totalPremium += componentValue;
            }
            else if (component.Operation.ToLower() == "subtract"){
                totalPremium -= componentValue;
            }

        }

        _logger.LogInformation($"{tag} Quote calculated successfully. Premium to be paid: {totalPremium}");
        var response = new {
            policyId = foundPolicy.Id,
            policy = foundPolicy.PolicyName,
            premium = totalPremium
        };

        return new ApiResponse<object>("200", "Quote retrieved successfully", response);
    }
}