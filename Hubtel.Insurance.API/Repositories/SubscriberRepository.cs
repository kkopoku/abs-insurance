namespace Hubtel.Insurance.API.Repositories;

using Hubtel.Insurance.API.Configurations;
using Hubtel.Insurance.API.Models;
using MongoDB.Driver;


public class SubscriberRepository(
    MongoDBContext mongoDBContext
):ISubscriberRepository{


    private readonly IMongoCollection<Subscriber> _subscribers = mongoDBContext.Subscribers;

    public async Task AddSubscriberAsync(Subscriber subscriber){
        await _subscribers.InsertOneAsync(subscriber);
        return;
    }

    public async Task<Subscriber> GetSubscriberByEmailAsync(string email){
        var found = await _subscribers.Find(s => s.Email == email).FirstOrDefaultAsync();
        return found;
    }


}