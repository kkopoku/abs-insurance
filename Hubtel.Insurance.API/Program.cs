using DotNetEnv;
using Hubtel.Insurance.API.Configurations;

var builder = WebApplication.CreateBuilder(args);
Env.Load();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.Configure<MongoDBSettings>(builder.Configuration.GetSection("MongoDB"));
builder.Services.AddSingleton<MongoDBContext>();


var mongoConnectionString = Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING");
var mongoDatabaseName = Environment.GetEnvironmentVariable("MONGO_DATABASE_NAME");

builder.Configuration["MongoDB:ConnectionString"] =
    mongoConnectionString ?? throw new Exception("MONGO_CONNECTION_STRING not set.");
builder.Configuration["MongoDB:DatabaseName"] =
    mongoDatabaseName ?? throw new Exception("MONGO_DATABASE_NAME not set.");


// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddEventSourceLogger();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers(); // Maps controller routes based on attribute routing in controllers


// create temp scope to test Database Connection
using (var scope = app.Services.CreateScope())
{
    var mongoDbContext = scope.ServiceProvider.GetRequiredService<MongoDBContext>();
    mongoDbContext.TestConnection();
}

app.Run();
