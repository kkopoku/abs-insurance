

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
using FluentValidation.TestHelper;

public class PolicyServiceBasicTests
{
    private readonly Mock<IPolicyRepository> _mockPolicyRepository;
    private readonly Mock<IPolicyComponentRepository> _mockPolicyComponentRepository;
    private readonly Mock<ILogger<PolicyService>> _mockLogger;
    private readonly PolicyService _policyService;
    private readonly UpdatePolicyValidator _updatePolicyValidator;



    public PolicyServiceBasicTests()
    {
        // Arrange
        _mockPolicyRepository = new Mock<IPolicyRepository>();
        _mockPolicyComponentRepository = new Mock<IPolicyComponentRepository>();
        _mockLogger = new Mock<ILogger<PolicyService>>();
        _updatePolicyValidator = new UpdatePolicyValidator();

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

        _mockPolicyRepository.Setup(repo => repo.GetByIdAsync(123)).ReturnsAsync(foundPolicy);

        // Act
        var result = await _policyService.CalculatePremium(requestQuoteDTO);
        Console.WriteLine(result);
        // Assert
        result.Code.Should().Be("200");
        result.Message.Should().Be("Quote retrieved successfully");

        var expectedPremium = 100 + 5000 + 50 - 2000; // = 3150

        var actualPremium = (result.Data).Premium;
        actualPremium.Should().Be(expectedPremium);
    }


    [Fact]
    public void ValidUpdatePolicyDTO_ShouldPassValidation()
    {
        var model = new UpdatePolicyDTO
        {
            PolicyId = "123456",
            PolicyName = "Updated Policy",
            Components = new List<UpdatePolicyComponentDTO>
            {
                new() { Sequence = 1, FlatValue = 100 }
            }
        };

        var result = _updatePolicyValidator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.PolicyId);
        result.ShouldNotHaveAnyValidationErrors();
    }



    [Fact]
    public void MissingPolicyId_ShouldFailValidation()
    {
        var model = (UpdatePolicyDTO)Activator.CreateInstance(typeof(UpdatePolicyDTO), nonPublic: true)!; // Dynamically creates an instance of UpdatePolicyDTO, bypassing required properties or private constructors.
        model.PolicyName = "Updated Policy";  // Only set PolicyName

        var result = _updatePolicyValidator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.PolicyId);
    }


    [Fact]
    public void EmptyComponents_ShouldPassValidation()
    {
        var model = (UpdatePolicyDTO)Activator.CreateInstance(typeof(UpdatePolicyDTO), nonPublic: true)!;
        model.PolicyId = "123456";
        model.PolicyName = "Updated Policy";
        model.Components = new List<UpdatePolicyComponentDTO>();

        var result = _updatePolicyValidator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }


    [Fact]
    public void SequenceOutOfRange_ShouldFailValidation()
    {
        var model = (UpdatePolicyDTO)Activator.CreateInstance(typeof(UpdatePolicyDTO), nonPublic: true)!;
        model.PolicyId = "123456";
        model.Components = new List<UpdatePolicyComponentDTO> { new() { Sequence = 5, FlatValue = 100 } };

        var result = _updatePolicyValidator.TestValidate(model);
        result.ShouldHaveValidationErrorFor("Components[0].Sequence");
    }


    [Fact]
    public void BothFlatAndPercentageValues_ShouldFailValidation()
    {
        var model = (UpdatePolicyDTO)Activator.CreateInstance(typeof(UpdatePolicyDTO), nonPublic: true)!;
        model.PolicyId = "123456";
        model.Components = new List<UpdatePolicyComponentDTO> { new() { Sequence = 2, FlatValue = 100, PercentageValue = 10 } };

        var result = _updatePolicyValidator.TestValidate(model);
        result.ShouldHaveAnyValidationError();
    }


    [Fact]
    public void NullComponents_ShouldPassValidation()
    {
        var model = (UpdatePolicyDTO)Activator.CreateInstance(typeof(UpdatePolicyDTO), nonPublic: true)!;
        model.PolicyId = "123456";
        model.Components = null;

        var result = _updatePolicyValidator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }


}