using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Hubtel.Insurance.API.Middlewares;

public class InvalidJsonMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<InvalidJsonMiddleware> _logger;

    public InvalidJsonMiddleware(RequestDelegate next, ILogger<InvalidJsonMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        const string tag = "[JSONMiddleware][Invoke]";
        _logger.LogInformation($"{tag} The JSON Middleware is running");

        if (context.Request.ContentType != null && context.Request.ContentType.Contains("application/json"))
        {
            try{
                // Enable buffering so we can read the stream multiple times
                context.Request.EnableBuffering();

                using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
                var requestBody = await reader.ReadToEndAsync();

                // Reset the stream position so the next middleware can read it
                context.Request.Body.Position = 0;

                // Try parsing JSON to validate it
                JsonConvert.DeserializeObject<object>(requestBody);
            }catch (JsonException ex){
                _logger.LogError($"[JSONMiddleware] Invalid JSON received: {ex.Message}");

                context.Response.StatusCode = 400;
                context.Response.ContentType = "application/json";

                var errorResponse = new
                {
                    code = "400",
                    message = "Invalid JSON format. Please check your request body.",
                    data = "error"
                };

                var errorJson = JsonConvert.SerializeObject(errorResponse);
                await context.Response.WriteAsync(errorJson, Encoding.UTF8);
                return; // Stop further request processing
            }
        }

        await _next(context);
    }
}