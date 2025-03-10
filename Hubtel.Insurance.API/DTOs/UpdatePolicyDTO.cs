using System.ComponentModel.DataAnnotations;

namespace Hubtel.Insurance.API.DTOs;

public class UpdatePolicyDTO
{
    public required string PolicyId { get; set; }
    public string? PolicyName { get; set; }
    public List<UpdatePolicyComponentDTO>? Components { get; set; }

}


public class UpdatePolicyComponentDTO
{
    [Range(1, 4, ErrorMessage = "Sequence must be between 1 and 4")]
    public required int Sequence { get; set; }

    public double? FlatValue { get; set; } = null;

    public double? PercentageValue { get; set; } = null;
    public string? PolicyId { get; set; }
}