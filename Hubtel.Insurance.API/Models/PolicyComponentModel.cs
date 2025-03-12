namespace Hubtel.Insurance.API.Models;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


public class PolicyComponent {

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }


    [BsonElement("policyId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public required string PolicyId { get; set; }


    [BsonElement("sequence")]
    public required int Sequence { get; set; }


    [BsonElement("name")]
    public required string Name { get; set; }


    [BsonElement("operation")]
    public required string Operation { get; set; }


    [BsonElement("flatValue")]
    public required double FlatValue { get; set; }
    

    [BsonElement("percentage")]
    public required double Percentage { get; set; }

}