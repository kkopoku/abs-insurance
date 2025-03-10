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

        var policies = await _policyRepository.GetAllAsync(pageNumber, pageSize);

        var paginatedResponse = new PaginatedResponse<GetPoliciesDTO>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            Data = policies
        };

        return new ApiResponse<PaginatedResponse<GetPoliciesDTO>>("200", "Policies fetched successfully", paginatedResponse);
    }


    public async Task<ApiResponse<Policy>> GetPolicyDetailsAsync(int id)
    {
        var tag = "[PolicyService][GetPolicyDetailsAsync]";

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


    public async Task<ApiResponse<object>> CalculatePremium(RequestQuoteDTO requestQuoteDTO)
    {
        var tag = "[PolicyService][CalculatePremium]";
        _logger.LogInformation($"{tag} Calculating premium ...");

        //  Find the policy
        var policyId = requestQuoteDTO.PolicyId;

        var foundPolicy = await _policyRepository.GetByIdAsync(policyId);

        if (foundPolicy == null)
        {
            _logger.LogInformation($"{tag} Policy not found");
            var notFoundMessage = "Policy not found, cannot request quote";
            return new ApiResponse<object>("404", notFoundMessage);
        }

        // Check if all 4 components are present
        var componentCount = foundPolicy.Components.Count;
        if (componentCount != 4)
        {
            _logger.LogInformation($"{tag} Policy components count is invalid. Count == {componentCount}");
            var errorMessage = "Cannot calculate quote for this policy";
            return new ApiResponse<object>("404", errorMessage);
        }

        double totalPremium = 0;
        foreach (var component in foundPolicy.Components.OrderBy(c => c.Sequence))
        {

            // check if component has both decimal and percentage
            if (component.FlatValue != 0 && component.Percentage != 0)
            {
                _logger.LogInformation($"{tag} Cannot have both flat value and percentage present. flatValue = {component.FlatValue} , percentage = {component.Percentage}");
                var errorMessage = "Something went wrong, please contact support";
                return new ApiResponse<object>("400", errorMessage);
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
        var response = new
        {
            policyId = foundPolicy.Id,
            policy = foundPolicy.PolicyName,
            premium = totalPremium
        };

        return new ApiResponse<object>("200", "Quote retrieved successfully", response);
    }



    public async Task<ApiResponse<object>> CreatePolicyAsync(CreatePolicyDTO policyDTO)
    {
        string tag = "[PolicyService][CreatePolicyAsync]";
        _logger.LogInformation($"{tag} Started processing request for Policy ID: {policyDTO.PolicyId}");

        // Validate component sequences
        _logger.LogInformation($"{tag} Validating policy components...");
        var components = policyDTO.Components;
        for (var i = 0; i < components.Count; i++)
        {
            var expectedCount = i + 1;
            var current = components[i];
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


    public async Task<ApiResponse<Policy>> UpdatePolicyAsync(UpdatePolicyDTO updateDto)
    {
        const string tag = "[PolicyService][UpdatePolicyAsync]";
        try{
            _logger.LogInformation($"{tag} Start processing request for Policy ID: {updateDto.PolicyId}");

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
                    if (updated){
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
        catch (Exception e){
            _logger.LogError($"{tag} An Error occurred. Error {e.Message}");
            return new ApiResponse<Policy>("500", "Something went wrong, please try again");
        }

    }


}