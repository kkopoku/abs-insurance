namespace Hubtel.Insurance.API.Models;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Policy {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public required string PolicyName { get; set; }
    public required int PolicyId { get; set; }
    
}