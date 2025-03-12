namespace Hubtel.Insurance.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Hubtel.Insurance.API.Services;
using Hubtel.Insurance.API.DTOs;
using Microsoft.AspNetCore.Authorization;

[Authorize]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class PolicyController(
    IPolicyService policyService,
    ILogger<PolicyController> logger
) : ControllerBase
{

    private readonly IPolicyService _policyService = policyService;
    private readonly ILogger<PolicyController> _logger = logger;


    [HttpGet]
    public async Task<IActionResult> GetAllPolicies(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10
    ){
        var tag = "[PolicyController][GetAllPolicies]";
        _logger.LogInformation($"{tag} Request received");
        var response = await _policyService.GetAllPolicies(pageNumber, pageSize);
        var code = int.Parse(response.Code);

        _logger.LogInformation($"{tag} Sending response: {JsonConvert.SerializeObject(response, Formatting.Indented)}");
        return StatusCode(code, response);
    }


    [HttpGet("{id}")]
    public async Task<IActionResult> GetPolicyDetails(int id)
    {
        var tag = "[PolicyController][GetPolicyDetails]";

        _logger.LogInformation($"{tag} Request recieved, id: {id}");
        var response = await _policyService.GetPolicyDetailsAsync(id);
        var code = int.Parse(response.Code);

        _logger.LogInformation($"{tag} Sending response: {JsonConvert.SerializeObject(response, Formatting.Indented)}");
        return StatusCode(code, response);
    }



    [HttpPost("request-quote")]
    public async Task<IActionResult> RequestQuote([FromBody] RequestQuoteDTO requestQuoteDTO)
    {
        var tag = "[PolicyController][RequestQuote]";

        _logger.LogInformation($"{tag} Request recieved: {JsonConvert.SerializeObject(requestQuoteDTO, Formatting.Indented)}");

        if (!ModelState.IsValid){
            _logger.LogInformation($"{tag} Invalid request body");

            string errors = string.Join(", ", ModelState.Values
            .Select(v => v.Errors[0].ErrorMessage));

            var badRequest = new ApiResponse<string>("400", errors);
            return StatusCode(400, badRequest);
        }

        var response = await _policyService.CalculatePremium(requestQuoteDTO);

        var code = int.Parse(response.Code);
        _logger.LogInformation($"{tag} Sending response: {JsonConvert.SerializeObject(response, Formatting.Indented)}");
        return StatusCode(code, response);
    }

    

    [HttpPost("create")]
    public async Task<IActionResult> CreatePolicy([FromBody] CreatePolicyDTO createPolicyDTO){
        var tag = "[PolicyController][CreatePolicy]";
        _logger.LogInformation($"{tag} Request recieved: {JsonConvert.SerializeObject(createPolicyDTO, Formatting.Indented)}");


        if (!ModelState.IsValid){
            _logger.LogInformation($"{tag} Invalid request body");

            string errors = string.Join(", ", ModelState.Values
            .Select(v => v.Errors[0].ErrorMessage));

            var badRequest = new ApiResponse<string>("400", errors);
            return StatusCode(400, badRequest);
        }

        var response = await _policyService.CreatePolicyAsync(createPolicyDTO);
        var code = int.Parse(response.Code);
        _logger.LogInformation($"{tag} Sending response: {JsonConvert.SerializeObject(response, Formatting.Indented)}");
        return StatusCode(code, response);
    }


    [HttpPatch("update")]
    public async Task<IActionResult> UpdatePolicy(
        [FromBody] UpdatePolicyDTO updateDto)
    {
        string tag = "[PolicyController][UpdatePolicy]";
        _logger.LogInformation($"{tag} Updating policy with ID: {updateDto.PolicyId}. Request: {JsonConvert.SerializeObject(updateDto, Formatting.Indented)}");

        if (!ModelState.IsValid){
            _logger.LogInformation($"{tag} Invalid request body");

            string errors = string.Join(", ", ModelState.Values
            .Select(v => v.Errors[0].ErrorMessage));

            var badRequest = new ApiResponse<string>("400", errors);
            return StatusCode(400, badRequest);
        }

        var response = await _policyService.UpdatePolicyAsync(updateDto);
        var code = int.Parse(response.Code);

        return StatusCode(code, response);
    }


    [HttpDelete("delete/{policyId}")]
    public async Task<IActionResult> DeletePolicy(int policyId){
        const string tag = "[PolicyController][DeletePolicy]";
        _logger.LogInformation($"{tag} Request received, with ID: {policyId}");

        var response = await _policyService.DeletePolicyByPolicyId(policyId.ToString());
        var code = int.Parse(response.Code);

        return StatusCode(code, response);
    }


}