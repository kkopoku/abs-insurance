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

public class CalculatePremiumTests
{
    private readonly Mock<IPolicyRepository> _mockPolicyRepository;
    private readonly Mock<IPolicyComponentRepository> _mockPolicyComponentRepository;
    private readonly Mock<ILogger<PolicyService>> _mockLogger;
    private readonly PolicyService _policyService;



    public CalculatePremiumTests()
    {
        // Arrange
        _mockPolicyRepository = new Mock<IPolicyRepository>();
        _mockPolicyComponentRepository = new Mock<IPolicyComponentRepository>();
        _mockLogger = new Mock<ILogger<PolicyService>>();

        // Act
        _policyService = new PolicyService(_mockPolicyRepository.Object, _mockPolicyComponentRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public void PolicyService_ShouldBeInitialized()
    {
        // Assert
        Assert.NotNull(_policyService);
    }


    [Fact]
    public async Task CalculatePremium_ShouldReturnCorrectPremium_WhenValidPolicy()
    {
        // Arrange
        var requestQuoteDTO = new RequestQuoteDTO { PolicyId = 123, MarketValue = 100000 };
        var foundPolicy = new Policy
        {
            Id = "67cdab421b22d93c9c3c94ee",
            PolicyId = 123,
            PolicyName = "Test Policy",
            Components = new List<PolicyComponent>
            {
                new PolicyComponent {Name="Premium Base", PolicyId="67cdab421b22d93c9c3c94ee", Sequence = 1, Operation = "add", FlatValue = 100, Percentage = 0 },
                new PolicyComponent {Name="Extra Perils", PolicyId="67cdab421b22d93c9c3c94ee", Sequence = 2, Operation = "add", FlatValue = 0, Percentage = 5 },  // 5% of 100000 = 5000
                new PolicyComponent {Name="Market Value Premium", PolicyId="67cdab421b22d93c9c3c94ee", Sequence = 3, Operation = "add", FlatValue = 50, Percentage = 0 },
                new PolicyComponent {Name="Discount", PolicyId="67cdab421b22d93c9c3c94ee", Sequence = 4, Operation = "subtract", FlatValue = 0, Percentage = 2 } // 2% of 100000 = 2000
            }
        };

        // mockPolicyRepo to return foundPolicy when id 123 is requested
        _mockPolicyRepository.Setup(repo => repo.GetByIdAsync(123)).ReturnsAsync(foundPolicy);

        // Act
        var result = await _policyService.CalculatePremium(requestQuoteDTO);

        // Assert
        result.Code.Should().Be("200");
        result.Message.Should().Be("Quote retrieved successfully");

        var expectedPremium = 100 + 5000 + 50 - 2000; // = 3150

        var actualPremium = (result.Data).Premium;
        actualPremium.Should().Be(expectedPremium);
    }

}