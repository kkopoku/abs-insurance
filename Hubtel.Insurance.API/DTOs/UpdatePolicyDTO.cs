using System.ComponentModel.DataAnnotations;

namespace Hubtel.Insurance.API.DTOs;

public class UpdatePolicyDTO
{
    public string PolicyId { get; set; }
    public string? PolicyName { get; set; }
    public List<UpdatePolicyComponentDTO>? Components { get; set; }

}


public class UpdatePolicyComponentDTO
{
    public int Sequence { get; set; }

    public double? FlatValue { get; set; } = null;

    public double? PercentageValue { get; set; } = null;
    public string? PolicyId { get; set; }
}