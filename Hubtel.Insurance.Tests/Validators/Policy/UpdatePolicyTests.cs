namespace Hubtel.Insurance.Tests.Validators;

using Xunit;
using Moq;
using Hubtel.Insurance.API.DTOs;
using FluentAssertions;
using Hubtel.Insurance.API.Validators;
using FluentValidation.TestHelper;

public class PolicyServiceBasicTests
{
    private readonly UpdatePolicyValidator _updatePolicyValidator;


    public PolicyServiceBasicTests()
    {
        // Arrange
        _updatePolicyValidator = new UpdatePolicyValidator();
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