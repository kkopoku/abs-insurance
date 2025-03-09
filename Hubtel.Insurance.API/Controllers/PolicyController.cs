namespace Hubtel.Insurance.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Hubtel.Insurance.API.Services;
using Hubtel.Insurance.API.DTOs;

[ApiController]
[Route("api/v1/[controller]")]
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
        _logger.LogInformation($"{tag} Request recieved");
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
            return StatusCode(400, new ApiResponse<string>("400","Invalid request body"));
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
            return StatusCode(400, new ApiResponse<string>("400","Invalid request body"));
        }

        var response = await _policyService.CreatePolicyAsync(createPolicyDTO);
        var code = int.Parse(response.Code);
        _logger.LogInformation($"{tag} Sending response: {JsonConvert.SerializeObject(response, Formatting.Indented)}");
        return StatusCode(code, response);
    }


}