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
using System.Collections.Generic;

public class UpdatePolicyTests
{
    private readonly Mock<IPolicyRepository> _mockPolicyRepository;
    private readonly Mock<IPolicyComponentRepository> _mockPolicyComponentRepository;
    private readonly Mock<ILogger<PolicyService>> _mockLogger;
    private readonly PolicyService _policyService;

    public UpdatePolicyTests()
    {
        _mockPolicyRepository = new Mock<IPolicyRepository>();
        _mockPolicyComponentRepository = new Mock<IPolicyComponentRepository>();
        _mockLogger = new Mock<ILogger<PolicyService>>();

        _policyService = new PolicyService(_mockPolicyRepository.Object, _mockPolicyComponentRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task UpdatePolicyAsync_ShouldFail_WhenComponentsHaveDuplicateSequences()
    {
        // Arrange
        var policyDTO = new UpdatePolicyDTO
        {
            PolicyId = "12345",
            PolicyName = "Updated Policy",
            Components = new List<UpdatePolicyComponentDTO>{
                new UpdatePolicyComponentDTO { Sequence = 1, FlatValue = 100 },
                new UpdatePolicyComponentDTO { Sequence = 1, PercentageValue = 5 },
                new UpdatePolicyComponentDTO { Sequence = 1, FlatValue = 200 }
            }
        };

        var existingPolicy = new Policy { Id = "67cdab421b22d93c9c3c94ee", PolicyId = 12345, PolicyName = "Existing Policy" };

        _mockPolicyRepository
            .Setup(repo => repo.GetByIdAsync(int.Parse(policyDTO.PolicyId)))
            .ReturnsAsync(existingPolicy); // Simulate policy exists

        // Act
        var result = await _policyService.UpdatePolicyAsync(policyDTO);

        // Assert
        result.Code.Should().Be("400");
        result.Message.Should().Contain("Duplicate sequence number detected for sequence");
        result.Data.Should().BeNull();
    }
}