namespace Hubtel.Insurance.API.DTOs;

using Hubtel.Insurance.API.Models;
using MongoDB.Bson.Serialization.Attributes;

public class GetPoliciesDTO {
    public required string Id { get; set; }

    [BsonElement("policyName")]
    public required string PolicyName { get; set; }

    [BsonElement("policyId")]
    public int PolicyId { get; set; }
    public List<PolicyComponent> Components { get; set; } = [];
}