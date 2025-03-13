namespace Hubtel.Insurance.API.Services;

using Hubtel.Insurance.API.DTOs;
using Hubtel.Insurance.API.Models;
using Hubtel.Insurance.API.Repositories;
using System.Text;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using MongoDB.Driver;

public class AuthService(
    ISubscriberRepository subscriberRepository,
    ILogger<AuthService> logger,
    IConfiguration configuration
    ) : IAuthService
{
    private readonly ISubscriberRepository _subscriberRepository = subscriberRepository;
    private readonly ILogger<AuthService> _logger = logger;
    private readonly IConfiguration _configuration = configuration;

    public async Task<ApiResponse<SubscriberResponseDTO>> RegisterAsync(RegisterSubscriberDTO registerUserDTO)
    {
        const string tag = "[AuthService][RegisterAsync]";
        try
        {
            // validate request
            if (string.IsNullOrEmpty(registerUserDTO.FirstName) ||
                string.IsNullOrEmpty(registerUserDTO.LastName) ||
                string.IsNullOrEmpty(registerUserDTO.Email) ||
                string.IsNullOrEmpty(registerUserDTO.Password))
            {
                _logger.LogWarning($"{tag} Missing required fields in registration request");
                return new ApiResponse<SubscriberResponseDTO>("400", "Missing required fields");
            }

            _logger.LogInformation($"{tag} Start processing registration for email: {registerUserDTO.Email}");

            var existingSubscriber = await _subscriberRepository.GetSubscriberByEmailAsync(registerUserDTO.Email);
            if (existingSubscriber != null)
            {
                _logger.LogWarning($"{tag} Email already registered: {registerUserDTO.Email}");
                return new ApiResponse<SubscriberResponseDTO>("400", "Email already registered");
            }

            // Validate password (should be more than 6 chars)
            if (registerUserDTO.Password.Length < 6)
            {
                _logger.LogWarning($"{tag} Password too short for email: {registerUserDTO.Email}");
                return new ApiResponse<SubscriberResponseDTO>("400", "Password length should be at least 6 characters.");
            }

            _logger.LogInformation($"{tag} Password validation passed for email: {registerUserDTO.Email}");

            var hashedPassword = HashPassword(registerUserDTO.Password);
            _logger.LogInformation($"{tag} Password hashed successfully for email: {registerUserDTO.Email}");

            var subscriber = new Subscriber
            {
                FirstName = registerUserDTO.FirstName,
                LastName = registerUserDTO.LastName,
                Email = registerUserDTO.Email,
                Password = hashedPassword
            };

            _logger.LogInformation($"{tag} Creating new subscriber record for email: {registerUserDTO.Email}");

            await _subscriberRepository.AddSubscriberAsync(subscriber);

            _logger.LogInformation($"{tag} Subscriber registration successful for email: {registerUserDTO.Email}");

            var responseDTO = new SubscriberResponseDTO
            {
                Id = subscriber.Id,
                FirstName = subscriber.FirstName,
                LastName = subscriber.LastName,
                Email = subscriber.Email
            };

            return new ApiResponse<SubscriberResponseDTO>("200", "Registration successful", responseDTO);
        }
        catch (Exception e)
        {
            _logger.LogError($"{tag} Error registering user {registerUserDTO.Email}: {e}");
            return new ApiResponse<SubscriberResponseDTO>("500", "An error occurred while registering the user. Please try again later.");
        }
    }


    public async Task<ApiResponse<string>> LoginAsync(SubscriberDTO subscriberDTO)
    {
        const string tag = "[AuthService][LoginAsync]";

        try{

            // validate request
            if(string.IsNullOrEmpty(subscriberDTO.Email) || string.IsNullOrEmpty(subscriberDTO.Password)){
                _logger.LogWarning($"{tag} Missing required fields in login request");
                return new ApiResponse<string>("400", "Missing required fields");
            }

            var subscriber = await _subscriberRepository.GetSubscriberByEmailAsync(subscriberDTO.Email);

            if (subscriber == null){
                _logger.LogWarning($"{tag} Email not found: {subscriberDTO.Email}");
                return new ApiResponse<string>("400", "Invalid email or password");
            }


            var passwordCheck = VerifyPassword(subscriberDTO.Password, subscriber.Password);
            if (!passwordCheck){
                _logger.LogInformation($"{tag} Incorrect password");
                return new ApiResponse<string>("400", "Invalid email or password");
            }


            // Generate a token for authorization
            string token = GenerateToken(subscriber);
            return new ApiResponse<string>("200", "Login successful", token);
        }catch (Exception e){
            _logger.LogError($"{tag} Error: {e.Message}");
            return new ApiResponse<string>("500", "An error occurred while attempting to login. Please try again later.");
        }
        
    }


    private static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    private static bool VerifyPassword(string inputPassword, string storedHash)
    {
        var match = BCrypt.Net.BCrypt.Verify(inputPassword, storedHash);
        return match;
    }


    public string GenerateToken(Subscriber subscriber)
    {
        ArgumentNullException.ThrowIfNull(subscriber);

        _logger.LogInformation(
            "[GenerateJwtToken] Generating JWT token for subscriber: {Email}",
            subscriber.Email
        );

        var jwtKey = _configuration["Jwt:Secret"] ?? throw new ArgumentNullException("Jwt:Secret");

        // sign JWT token
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, subscriber.FirstName),
            new Claim(ClaimTypes.Email, subscriber.Email)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24), // Use UtcNow
            signingCredentials: credentials
        );

        _logger.LogInformation(
            "[GenerateJwtToken] JWT token generated successfully for subscriber: {Email}",
            subscriber.Email
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}