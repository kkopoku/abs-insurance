namespace Hubtel.Insurance.API.DTOs;

using Hubtel.Insurance.API.Models;

public class GetPoliciesDTO {
    public required string Id { get; set; }
    public required string PolicyName { get; set; }
    public int PolicyId { get; set; }
    public List<PolicyComponent> Components { get; set; } = [];
}