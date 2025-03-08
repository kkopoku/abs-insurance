namespace Hubtel.Insurance.API.Controllers;

using Microsoft.AspNetCore.Mvc;


[ApiController]
[Route("api/v1/[controller]")]
public class PolicyController() : ControllerBase {


    [HttpGet]
    public async Task<IActionResult> Get() {
        await Task.Delay(500);
        return Ok(new { message = "Get all policies" });
    }

}