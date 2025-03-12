namespace Hubtel.Insurance.API.Models;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Policy {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }


    [BsonElement("policyName")]
    public required string PolicyName { get; set; }


    [BsonElement("policyId")]
    public required int PolicyId { get; set; }


    [BsonIgnore] // ignore, mongo doesnt save this field
    public List<PolicyComponent> Components { get; set; } = [];
    
}