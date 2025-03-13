namespace Hubtel.Insurance.Tests.Services.Policy;

using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Hubtel.Insurance.API.Services;
using Hubtel.Insurance.API.Repositories;
using Hubtel.Insurance.API.DTOs;
using Hubtel.Insurance.API.Models;
using FluentAssertions;
using System.Text.Json;
using Hubtel.Insurance.API.Validators;

public class CreatePolicyTests
{
    private readonly Mock<IPolicyRepository> _mockPolicyRepository;
    private readonly Mock<IPolicyComponentRepository> _mockPolicyComponentRepository;
    private readonly Mock<ILogger<PolicyService>> _mockLogger;
    private readonly PolicyService _policyService;


    public CreatePolicyTests()
    {
        // Arrange
        _mockPolicyRepository = new Mock<IPolicyRepository>();
        _mockPolicyComponentRepository = new Mock<IPolicyComponentRepository>();
        _mockLogger = new Mock<ILogger<PolicyService>>();

        // Act
        _policyService = new PolicyService(_mockPolicyRepository.Object, _mockPolicyComponentRepository.Object, _mockLogger.Object);
    }



    [Fact]
    public async Task CreatePolicyAsync_ShouldReturnSuccess_WhenValidPolicyIsCreated()
    {

        // describe request
        var policyDTO = new CreatePolicyDTO
        {
            PolicyId = 12345,
            Policy = "Test Policy",
            Components = new List<PolicyComponentInputDTO>{
                new PolicyComponentInputDTO { Sequence = 2, PercentageValue = 5 },
                new PolicyComponentInputDTO { Sequence = 1, FlatValue = 100 },
                new PolicyComponentInputDTO { Sequence = 3, FlatValue = 100 },
                new PolicyComponentInputDTO { Sequence = 4, FlatValue = 100 }
            }
        };

        var createdPolicy = new Policy { Id = "67cdab421b22d93c9c3c94ee", PolicyId = 12345, PolicyName = "Test Policy" };

        _mockPolicyRepository
            .Setup(repo => repo.GetByIdAsync(policyDTO.PolicyId))
            .ReturnsAsync((Policy)null); // Simulate policy not existing

        _mockPolicyRepository
            .Setup(repo => repo.CreateAsync(It.IsAny<Policy>()))
            .ReturnsAsync(createdPolicy); // Simulate successful policy creation

        _mockPolicyComponentRepository
            .Setup(repo => repo.CreateAsync(It.IsAny<PolicyComponent>()))
            .ReturnsAsync((PolicyComponent component) => component); // Simulate successful component creation

        var service = new PolicyService(_mockPolicyRepository.Object, _mockPolicyComponentRepository.Object, _mockLogger.Object);

        // Act
        var result = await service.CreatePolicyAsync(policyDTO);

        // Assert
        Assert.Equal("200", result.Code);
        Assert.Equal("Policy successfully created", result.Message);
        Assert.NotNull(result.Data);
    }


}