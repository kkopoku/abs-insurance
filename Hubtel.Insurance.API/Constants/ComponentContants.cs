namespace Hubtel.Insurance.API.Constants;

public static class ComponentConstants
{
    public static readonly Dictionary<int, string[]> ComponentDefinitions = new()
    {
        { 1, new[] { "Premium Base", "add" } },
        { 2, new[] { "Extra Perils", "add" } },
        { 3, new[] { "Market Value Premium", "add" } },
        { 4, new[] { "Promo Discount", "subtract" } }
    };
}