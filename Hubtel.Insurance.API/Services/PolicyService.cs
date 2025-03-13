namespace Hubtel.Insurance.API.Services;

using Hubtel.Insurance.API.Repositories;
using Hubtel.Insurance.API.DTOs;
using Hubtel.Insurance.API.Models;
using Hubtel.Insurance.API.Constants;
using Newtonsoft.Json;

public class PolicyService(
    IPolicyRepository policyRepository,
    IPolicyComponentRepository policyComponentRepository,
    ILogger<PolicyService> logger
) : IPolicyService
{

    private readonly IPolicyRepository _policyRepository = policyRepository;
    private readonly IPolicyComponentRepository _policyComponentRepository = policyComponentRepository;
    private readonly ILogger<PolicyService> _logger = logger;


    public async Task<ApiResponse<PaginatedResponse<GetPoliciesDTO>>> GetAllPolicies(int pageNumber, int pageSize)
    {
        const string tag = "[PolicyService][GetAllPolicies]";
        _logger.LogInformation($"{tag} Fetching policies - pageNumber: {pageNumber}, pageSize: {pageSize}");

        try
        {
            var policies = await _policyRepository.GetAllAsync(pageNumber, pageSize);
            var paginatedResponse = new PaginatedResponse<GetPoliciesDTO>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                Data = policies
            };

            return new ApiResponse<PaginatedResponse<GetPoliciesDTO>>("200", "Policies fetched successfully", paginatedResponse);
        }
        catch (Exception e)
        {
            _logger.LogError($"{tag} Error fetching policies. Error: {e.Message}");
            return new ApiResponse<PaginatedResponse<GetPoliciesDTO>>("500", "An error occurred while creating the policy. Please try again later.");

        }
    }


    public async Task<ApiResponse<Policy>> GetPolicyDetailsAsync(int id)
    {
        var tag = "[PolicyService][GetPolicyDetailsAsync]";

        try
        {
            _logger.LogInformation($"{tag} Searching for policy with id: {id}");
            var foundPolicy = await _policyRepository.GetByIdAsync(id);
            if (foundPolicy == null)
            {
                _logger.LogInformation($"{tag} Policy with id: {id} not found, returning not found response");
                var notFoundMessage = "Policy not found";
                var response = new ApiResponse<Policy>("404", notFoundMessage);
                return response;
            }
            _logger.LogInformation($"{tag} Policy with id: {id} found, returning success response");
            var successMessage = "Policy fetched successfully";
            return new ApiResponse<Policy>("200", successMessage, foundPolicy);
        }
        catch (Exception e)
        {
            _logger.LogError($"{tag} Error fetching policy details. Error: {e.Message}");
            return new ApiResponse<Policy>("500", "An error occurred while fetching policy details. Please try again later.");
        }
    }


    public async Task<ApiResponse<CalculatePremiumDTO>> CalculatePremium(RequestQuoteDTO requestQuoteDTO)
    {
        var tag = "[PolicyService][CalculatePremium]";
        _logger.LogInformation($"{tag} Calculating premium ...");

        try
        {
            // Find the policy
            var policyId = requestQuoteDTO.PolicyId;
            var foundPolicy = await _policyRepository.GetByIdAsync(policyId);

            if (foundPolicy == null)
            {
                _logger.LogInformation($"{tag} Policy not found");
                return new ApiResponse<CalculatePremiumDTO>("404", "Policy not found, cannot request quote");
            }

            // Check if all 4 components are present
            var componentCount = foundPolicy.Components.Count;
            if (componentCount != 4)
            {
                _logger.LogInformation($"{tag} Policy components count is invalid. Count == {componentCount}");
                return new ApiResponse<CalculatePremiumDTO>("404", "Cannot calculate quote for this policy");
            }

            double totalPremium = 0;
            foreach (var component in foundPolicy.Components.OrderBy(c => c.Sequence))
            {
                // Check if component has both flat value and percentage
                if (component.FlatValue != 0 && component.Percentage != 0)
                {
                    _logger.LogInformation($"{tag} Cannot have both flat value and percentage present. flatValue = {component.FlatValue} , percentage = {component.Percentage}");
                    return new ApiResponse<CalculatePremiumDTO>("400", "Something went wrong, please contact support");
                }

                double componentValue = component.FlatValue + (requestQuoteDTO.MarketValue * component.Percentage / 100);
                if (component.Operation.ToLower() == "add")
                {
                    totalPremium += componentValue;
                }
                else if (component.Operation.ToLower() == "subtract")
                {
                    totalPremium -= componentValue;
                }
            }

            _logger.LogInformation($"{tag} Quote calculated successfully. Premium to be paid: {totalPremium}");
            CalculatePremiumDTO response = new()
            {
                PolicyId = foundPolicy.Id,
                Policy = foundPolicy.PolicyName,
                Premium = totalPremium
            };

            return new ApiResponse<CalculatePremiumDTO>("200", "Quote retrieved successfully", response);
        }
        catch (Exception ex)
        {
            _logger.LogError($"{tag} An error occurred while calculating premium: {ex.Message}", ex);
            return new ApiResponse<CalculatePremiumDTO>("500", "An unexpected error occurred while calculating the premium");
        }
    }




    public async Task<ApiResponse<object>> CreatePolicyAsync(CreatePolicyDTO policyDTO)
    {
        string tag = "[PolicyService][CreatePolicyAsync]";
        _logger.LogInformation($"{tag} Started processing request for Policy ID: {policyDTO.PolicyId}");

        try
        {
            // Validate component sequences
            _logger.LogInformation($"{tag} Validating policy components...");
            var components = policyDTO.Components;

            var componentSorted = components.OrderBy(a => a.Sequence).ToList();

            _logger.LogInformation($"{tag} Here: {JsonConvert.SerializeObject(componentSorted, Formatting.Indented)}");

            for (var i = 0; i < componentSorted.Count; i++)
            {
                var expectedCount = i + 1;
                var current = componentSorted[i];
                if (expectedCount != current.Sequence)
                {
                    _logger.LogWarning($"{tag} Invalid component sequence detected at index {i}. Expected: {expectedCount}, Found: {current.Sequence}");
                    return new ApiResponse<object>("400", $"Invalid policy components, missing component with sequence {expectedCount}");
                }
                else if ((current.PercentageValue ?? 0) != 0 && (current.FlatValue ?? 0) != 0)
                {
                    _logger.LogWarning($"{tag} FlatValue and Percentage present abort ... flatValue: {current.FlatValue} percentage: {current.PercentageValue}");
                    return new ApiResponse<object>("400", "Invalid policy components. Cannot have both percentage and flatValue in a component");
                }
            }
            _logger.LogInformation($"{tag} Policy components validated successfully");

            // Check if policy ID already exists
            _logger.LogInformation($"{tag} Checking if Policy ID {policyDTO.PolicyId} already exists...");
            var check = await _policyRepository.GetByIdAsync(policyDTO.PolicyId);
            if (check != null)
            {
                _logger.LogWarning($"{tag} Policy ID {policyDTO.PolicyId} already exists.");
                return new ApiResponse<object>("400", "Policy with this ID already exists");
            }

            // Save policy in database
            _logger.LogInformation($"{tag} Creating new policy with ID: {policyDTO.PolicyId}");
            Policy toCreate = new()
            {
                PolicyId = policyDTO.PolicyId,
                PolicyName = policyDTO.Policy
            };

            var createdPolicy = await _policyRepository.CreateAsync(toCreate);
            if (createdPolicy == null)
            {
                _logger.LogError($"{tag} Failed to create policy for ID {policyDTO.PolicyId}");
                return new ApiResponse<object>("400", "Policy could not be created");
            }
            _logger.LogInformation($"{tag} Policy created successfully with ID {createdPolicy.Id}");

            // Create policy components
            _logger.LogInformation($"{tag} Creating components for Policy ID {createdPolicy.Id}...");
            List<PolicyComponent> createdComponents = new();

            foreach (var component in components)
            {
                if (!ComponentConstants.ComponentDefinitions.TryGetValue(component.Sequence, out var componentInfo))
                {
                    _logger.LogWarning($"{tag} Invalid sequence number {component.Sequence} in policy ID {createdPolicy.Id}");
                    return new ApiResponse<object>("400", $"Invalid sequence number: {component.Sequence}");
                }

                var componentToCreate = new PolicyComponent
                {
                    PolicyId = createdPolicy.Id,
                    Sequence = component.Sequence,
                    Name = componentInfo[0], // Name from dictionary
                    Operation = componentInfo[1], // Operation from dictionary
                    FlatValue = component.FlatValue ?? 0,
                    Percentage = component.PercentageValue ?? 0
                };

                var createdComponent = await _policyComponentRepository.CreateAsync(componentToCreate);
                if (createdComponent == null)
                {
                    _logger.LogError($"{tag} Failed to create component with sequence {component.Sequence} for Policy ID {createdPolicy.Id}");
                    return new ApiResponse<object>("400", $"Failed to create component with sequence {component.Sequence}");
                }

                _logger.LogInformation($"{tag} Created component '{componentToCreate.Name}' with sequence {component.Sequence} for Policy ID {createdPolicy.Id}");
                createdComponents.Add(createdComponent);
            }

            createdPolicy.Components = createdComponents;
            _logger.LogInformation($"{tag} Policy creation completed successfully for Policy ID {createdPolicy.Id}");

            return new ApiResponse<object>("200", "Policy successfully created", createdPolicy);
        }
        catch (Exception ex)
        {
            _logger.LogError($"{tag} An unexpected error occurred while creating policy: {ex.Message}");
            return new ApiResponse<object>("500", "An error occurred while creating the policy. Please try again later.");
        }
    }



    public async Task<ApiResponse<Policy>> UpdatePolicyAsync(UpdatePolicyDTO updateDto)
    {
        const string tag = "[PolicyService][UpdatePolicyAsync]";
        try
        {
            _logger.LogInformation($"{tag} Start processing request for Policy ID: {updateDto.PolicyId}");

            // Validate request
            var componentSorted = updateDto.Components.OrderBy(a => a.Sequence).ToList();

            _logger.LogInformation($"{tag} Here: {JsonConvert.SerializeObject(componentSorted, Formatting.Indented)}");

            for (var i = 0; i < componentSorted.Count; i++){
                var expectedCount = i + 1;
                var current = componentSorted[i];
                if (expectedCount != current.Sequence){
                    _logger.LogWarning($"{tag} Invalid component sequence detected at index {i}. Expected: {expectedCount}, Found: {current.Sequence}");
                    if(expectedCount > current.Sequence){
                        return new ApiResponse<Policy>("400", $"Duplicate sequence number detected for sequence: {current.Sequence}, please try again");
                    }
                    return new ApiResponse<Policy>("400", "Invalid policy components, please try again");
                }
            }


            // Find policy
            var foundPolicy = await _policyRepository.GetByIdAsync(int.Parse(updateDto.PolicyId));
            _logger.LogInformation($"{tag} Found policy: {JsonConvert.SerializeObject(foundPolicy, Formatting.Indented)}");

            if (foundPolicy == null)
            {
                _logger.LogInformation($"{tag} Policy with ID: {updateDto.PolicyId} doesn't exist");
                return new ApiResponse<Policy>("404", "Policy not found");
            }

            bool policyUpdated = false;

            // Check if PolicyName is in request and update
            if (!string.IsNullOrEmpty(updateDto.PolicyName))
            {
                _logger.LogInformation($"{tag} Updating policy name from {foundPolicy.PolicyName} to {updateDto.PolicyName}");
                policyUpdated = await _policyRepository.UpdateAsync(updateDto);
            }

            bool componentsUpdated = false;

            // Update components if passed in request
            if (updateDto.Components != null && updateDto.Components.Count > 0)
            {
                foreach (var component in updateDto.Components)
                {
                    _logger.LogInformation($"{tag} Updating component Sequence: {component.Sequence}");
                    component.PolicyId = foundPolicy.Id;
                    var updated = await _policyComponentRepository.UpdatePolicyComponentAsync(component);
                    if (updated)
                    {
                        componentsUpdated = true;
                    }
                }
            }

            if (!policyUpdated && !componentsUpdated)
            {
                _logger.LogInformation($"{tag} No updates were made");
                return new ApiResponse<Policy>("400", "No valid fields to update");
            }

            var updatedPolicy = await _policyRepository.GetByIdAsync(int.Parse(updateDto.PolicyId));

            _logger.LogInformation($"{tag} Policy update completed successfully");
            return new ApiResponse<Policy>("200", "Policy updated successfully", updatedPolicy);
        }
        catch (Exception e)
        {
            _logger.LogError($"{tag} An Error occurred. Error {e.Message}");
            return new ApiResponse<Policy>("500", "Something went wrong, please try again");
        }

    }


    public async Task<ApiResponse<string>> DeletePolicyByPolicyId(string policyId)
    {
        const string tag = "[PolicyService][DeletePolicyByPolicyId]";
        _logger.LogInformation($"{tag} Start deleting policy with PolicyId: {policyId}");

        try
        {
            // Check if the policy exists
            var policy = await _policyRepository.GetByIdAsync(int.Parse(policyId));
            if (policy == null)
            {
                _logger.LogWarning($"{tag} No policy found with PolicyId: {policyId}");
                return new ApiResponse<string>("404", "Policy not found");
            }

            // Delete all related policy components
            var deletedComponents = await _policyComponentRepository.DeletePolicyComponentsByPolicyIdAsync(policy.Id);
            _logger.LogInformation($"{tag} Deleted {deletedComponents} components associated with PolicyId: {policyId}");

            // Delete the policy itself
            var isDeleted = await _policyRepository.DeletePolicyByPolicyIdAsync(policyId);
            if (!isDeleted)
            {
                _logger.LogError($"{tag} Failed to delete policy with PolicyId: {policyId}");
                return new ApiResponse<string>("500", "Failed to delete policy, please try again");
            }

            _logger.LogInformation($"{tag} Successfully deleted policy with PolicyId: {policyId}");
            return new ApiResponse<string>("200", "Policy deleted successfully");
        }
        catch (Exception e)
        {
            _logger.LogError($"{tag} An Error occurred. Error: {e.Message}");
            return new ApiResponse<string>("500", "Something went wrong, please try again");
        }
    }


}