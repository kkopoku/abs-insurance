namespace Hubtel.Insurance.API.Models;

using MongoDB.Bson.Serialization.Attributes;

public class Subscriber {

    [BsonId]
    [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
    public string Id {get; set;}


    [BsonElement("firstName")]
    public string FirstName {get; set;}


    [BsonElement("lastName")]
    public string LastName {get; set;}


    [BsonElement("email")]
    public string Email {get; set;}


    [BsonElement("password")]
    public string Password {get; set;}


    [BsonElement("createdAt")]
    public DateTime CreatedAt {get; set;}

}