using DotNetEnv;
using Hubtel.Insurance.API.Configurations;
using Hubtel.Insurance.API.Repositories;
using Hubtel.Insurance.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Hubtel.Insurance.API.Validators;
using Hubtel.Insurance.API.Middlewares;


var builder = WebApplication.CreateBuilder(args);
Env.Load();

// Load environment variables
var mongoConnectionString = Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING") 
    ?? throw new Exception("MONGO_CONNECTION_STRING not set.");
var mongoDatabaseName = Environment.GetEnvironmentVariable("MONGO_DATABASE_NAME") 
    ?? throw new Exception("MONGO_DATABASE_NAME not set.");
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") 
    ?? throw new Exception("JWT_SECRET not set.");
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") 
    ?? throw new Exception("JWT_ISSUER not set.");
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") 
    ?? throw new Exception("JWT_AUDIENCE not set.");

// Set configuration values
builder.Configuration["MongoDB:ConnectionString"] = mongoConnectionString;
builder.Configuration["MongoDB:DatabaseName"] = mongoDatabaseName;
builder.Configuration["Jwt:Secret"] = jwtSecret;
builder.Configuration["Jwt:Issuer"] = jwtIssuer;
builder.Configuration["Jwt:Audience"] = jwtAudience;

// API versioning
builder.Services.AddApiVersioning(options =>
{
    options.ReportApiVersions = true;
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});

// Add services
builder.Services.AddOpenApi();
builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
    {
        // Disabling default model validation behavior
        options.SuppressModelStateInvalidFilter = true;
    });
;

builder.Services.Configure<MongoDBSettings>(builder.Configuration.GetSection("MongoDB"));
builder.Services.AddSingleton<MongoDBContext>();
builder.Services.AddScoped<DatabaseSeeder>();
builder.Services.AddScoped<IPolicyRepository, PolicyRepository>();
builder.Services.AddScoped<IPolicyComponentRepository, PolicyComponentRepository>();
builder.Services.AddScoped<ISubscriberRepository, SubscriberRepository>();
builder.Services.AddScoped<IPolicyService, PolicyService>();
builder.Services.AddScoped<IAuthService, AuthService>();


// Register FluentValidation services
builder.Services.AddFluentValidationAutoValidation(options => options.DisableDataAnnotationsValidation = true)
                .AddFluentValidationClientsideAdapters();


// Register all validators in the assembly
builder.Services.AddValidatorsFromAssemblyContaining<RegisterSubscriberValidator>();



// Register JWT authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero // Optional: removes default 5-minute token expiry leeway
        };
    });

builder.Services.AddAuthorization();

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddEventSourceLogger();

var app = builder.Build();

// Register the custom middleware
app.UseMiddleware<InvalidJsonMiddleware>();

// Handle database seeding
if (args.Length > 0 && args[0].ToLower() == "seed")
{
    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();

    try
    {
        Console.WriteLine("Seeding database...");
        await seeder.SeedAsync();
        Console.WriteLine("Database seeding completed.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error seeding database: {ex.Message}");
    }

    return; // Exit after seeding
}

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


// Handle 401 unauthorized requests by returning custom error message
app.Use(async (context, next) =>
{
    Console.WriteLine(" Middleware is called");
    await next();

    if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync("{\"code\":\"401\",\"message\":\"Unauthorized access\"}");
    }
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Test database connection
using (var scope = app.Services.CreateScope())
{
    var mongoDbContext = scope.ServiceProvider.GetRequiredService<MongoDBContext>();
    mongoDbContext.TestConnection();
}

app.Run();