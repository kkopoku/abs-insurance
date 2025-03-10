namespace Hubtel.Insurance.API.DTOs;


public class CalculatePremiumDTO {

    public required string PolicyId { get; set;}
    public required string Policy { get; set;}
    public required double Premium { get; set; }

}