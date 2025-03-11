using Microsoft.AspNetCore.Mvc;
using Hubtel.Insurance.API.Services;
using Hubtel.Insurance.API.DTOs;
using Newtonsoft.Json;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;

namespace Hubtel.Insurance.API.Controllers;



[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class AuthController(
    IAuthService authService,
    ILogger<AuthController> logger
) : ControllerBase
{

    private readonly ILogger<AuthController> _logger = logger;
    private readonly IAuthService _authService = authService;


    [HttpPost("register")]
    public async Task<IActionResult> RegisterSubscriberAsync(
        [FromBody] RegisterSubscriberDTO registerSubscriberDTO
        )
    {
        const string tag = "[AuthController][RegisterSubscriberAsync]";
        _logger.LogInformation($"Attempt to register user with email {registerSubscriberDTO.Email}");


        if (!ModelState.IsValid){
            _logger.LogInformation($"{tag} Invalid request body");

            string errors = string.Join(", ", ModelState.Values
            .Select(v => v.Errors[0].ErrorMessage));

            var badRequest = new ApiResponse<string>("400", errors);
            return StatusCode(400, badRequest);
        }

        var response = await _authService.RegisterAsync(registerSubscriberDTO);
        var code = int.Parse(response.Code);

        _logger.LogInformation($"{tag} Sending response: {JsonConvert.SerializeObject(response, Formatting.Indented)}");
        return StatusCode(code, response);
    }



    [HttpPost("login")]
    public async Task<IActionResult> LoginSubscriberAsync([FromBody] SubscriberDTO subscriberDTO)
    {
        const string tag = "[AuthController][LoginSubscriberAsync]";
        _logger.LogInformation($"Attempting to login user with email {subscriberDTO.Email}");

        if (!ModelState.IsValid){
            _logger.LogInformation($"{tag} Invalid request body");

            string errors = string.Join(", ", ModelState.Values
            .Select(v => v.Errors[0].ErrorMessage));

            var badRequest = new ApiResponse<string>("400", errors);
            return StatusCode(400, badRequest);
        }


        var response = await _authService.LoginAsync(subscriberDTO);
        var code = int.Parse(response.Code);

        _logger.LogInformation($"{tag} Sending response: {JsonConvert.SerializeObject(response, Formatting.Indented)}");
        return StatusCode(code, response);
    }

}