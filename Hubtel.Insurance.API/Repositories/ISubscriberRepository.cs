namespace Hubtel.Insurance.API.Repositories;

using Hubtel.Insurance.API.Models;
// using Hubtel.Insurance.API.

public interface ISubscriberRepository {

    Task AddSubscriberAsync(Subscriber subscriber);
    Task<Subscriber> GetSubscriberByEmailAsync(string email);


}