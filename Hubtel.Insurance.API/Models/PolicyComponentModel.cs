namespace Hubtel.Insurance.API.Models;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


public class PolicyComponent {

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public required string PolicyId { get; set; }

    public required int Sequence { get; set; }

    public required string Name { get; set; }

    public required string Operation { get; set; }

    public required double FlatValue { get; set; }
    
    public required double Percentage { get; set; }

}