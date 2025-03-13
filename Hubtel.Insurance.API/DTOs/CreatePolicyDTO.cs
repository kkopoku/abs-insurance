namespace Hubtel.Insurance.API.DTOs;

using System.ComponentModel.DataAnnotations;

public class CreatePolicyDTO
{
    public int PolicyId { get; set; }

    public string Policy { get; set; }

    public List<PolicyComponentInputDTO> Components { get; set; }
}

public class PolicyComponentInputDTO
{
    public int Sequence { get; set; }

    public double? FlatValue { get; set; }

    public double? PercentageValue { get; set; }
}