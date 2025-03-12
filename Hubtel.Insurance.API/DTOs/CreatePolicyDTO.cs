namespace Hubtel.Insurance.API.DTOs;

using System.ComponentModel.DataAnnotations;

public class CreatePolicyDTO
{
    [Required(ErrorMessage = "Policy Id is required")]
    public int PolicyId { get; set; }

    [Required(ErrorMessage = "Policy name is required")]
    public string Policy { get; set; }

    [Required(ErrorMessage = "Exactly 4 components are required")]
    [MinLength(4, ErrorMessage = "Exactly 4 components are required")]
    [MaxLength(4, ErrorMessage = "Exactly 4 components are required")]
    public List<PolicyComponentInputDTO> Components { get; set; }
}

public class PolicyComponentInputDTO
{
    [Required(ErrorMessage = "Sequence is required")]
    [Range(1, 4, ErrorMessage = "Sequence must be between 1 and 4")]
    public int Sequence { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Flat value cannot be negative")]
    public double? FlatValue { get; set; }

    [Range(0, 100, ErrorMessage = "Percentage value must be between 0 and 100")]
    public double? PercentageValue { get; set; }
}