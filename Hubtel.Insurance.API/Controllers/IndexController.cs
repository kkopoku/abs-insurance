namespace Hubtel.Insurance.API.Controllers;


using Microsoft.AspNetCore.Mvc;
using Hubtel.Insurance.API.DTOs;


[ApiController]
[Route("/")]
public class IndexController:ControllerBase
{
    [HttpGet]
    public IActionResult TestWaters(){
        var message = "Service is up and running";
        return StatusCode(200, new ApiResponse<string>("200", message));
    }
}