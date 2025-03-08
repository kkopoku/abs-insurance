namespace Hubtel.Insurance.API.DTOs;

public class SeedPolicyDto{
    public int PolicyId { get; set; }
    public string PolicyName { get; set; } = string.Empty;
    public List<SeedPolicyComponentDTO> Components { get; set; } = new();
}

public class SeedPolicyComponentDTO
{
    public int Sequence { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public double FlatValue { get; set; }
    public double Percentage { get; set; }
}