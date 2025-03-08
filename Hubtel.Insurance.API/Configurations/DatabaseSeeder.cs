using Hubtel.Insurance.API.Repositories;
using Hubtel.Insurance.API.Models;
using Hubtel.Insurance.API.DTOs;
using MongoDB.Bson;

namespace Hubtel.Insurance.API.Configurations;

public class DatabaseSeeder
{
    private readonly IPolicyRepository _policyRepository;
    private readonly IPolicyComponentRepository _policyComponentRepository;

    public DatabaseSeeder(
        IPolicyRepository policyRepository,
        IPolicyComponentRepository policyComponentRepository)
    {
        _policyRepository = policyRepository;
        _policyComponentRepository = policyComponentRepository;
    }

    public async Task SeedAsync()
    {
        var existingPolicies = await _policyRepository.GetAllAsync();
        if (existingPolicies.Count != 0)
        {
            Console.WriteLine("Database already seeded.");
            return;
        }

        var policiesData = new List<SeedPolicyDto>
        {
            new() {
                PolicyName = "Low Claim Policy",
                Components =
                {
                    new() { Sequence = 1, Name = "Premium Base", Operation = "add", FlatValue = 300.00, Percentage = 0 },
                    new() { Sequence = 2, Name = "Extra Perils", Operation = "add", FlatValue = 100.00, Percentage = 0 },
                    new() { Sequence = 3, Name = "Market Value Premium", Operation = "add", FlatValue = 0, Percentage = 10 },
                    new() { Sequence = 4, Name = "Promo Discount", Operation = "subtract", FlatValue = 0, Percentage = 0 }
                }
            },
            new()
            {
                PolicyId = 2,
                PolicyName = "Medium Claim Policy",
                Components =
                {
                    new() { Sequence = 1, Name = "Premium Base", Operation = "add", FlatValue = 500.00, Percentage = 0 },
                    new() { Sequence = 2, Name = "Extra Perils", Operation = "add", FlatValue = 250.00, Percentage = 0 },
                    new() { Sequence = 3, Name = "Market Value Premium", Operation = "add", FlatValue = 0, Percentage = 12 },
                    new() { Sequence = 4, Name = "Promo Discount", Operation = "subtract", FlatValue = 50, Percentage = 0 }
                }
            },
            new()
            {
                PolicyId = 3,
                PolicyName = "High Claim Policy",
                Components =
                {
                    new() { Sequence = 1, Name = "Premium Base", Operation = "add", FlatValue = 1000.00, Percentage = 0 },
                    new() { Sequence = 2, Name = "Extra Perils", Operation = "add", FlatValue = 500.00, Percentage = 0 },
                    new() { Sequence = 3, Name = "Market Value Premium", Operation = "add", FlatValue = 350, Percentage = 15 },
                    new() { Sequence = 4, Name = "Promo Discount", Operation = "subtract", FlatValue = 120, Percentage = 0 }
                }
            }
        };

        foreach (var policy in policiesData)
        {
            var policyRecord = new Policy {
                PolicyId = policy.PolicyId,
                PolicyName = policy.PolicyName
            };

            var createdPolicy = await _policyRepository.CreateAsync(policyRecord);


            foreach (var component in policy.Components)
            {
                var policyComponentRecord = new PolicyComponent {
                    PolicyId = ObjectId.Parse(policyRecord.Id),
                    Sequence = component.Sequence,
                    Name = component.Name,
                    Operation = component.Operation,
                    FlatValue = component.FlatValue,
                    Percentage = component.Percentage
                };

                await _policyComponentRepository.CreateAsync(policyComponentRecord);
            }
        }

        Console.WriteLine("Database seeding completed successfully.");
    }
}
