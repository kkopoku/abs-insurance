namespace Hubtel.Insurance.API.DTOs;

using System.ComponentModel.DataAnnotations;

public class RequestQuoteDTO
{
    [Required(ErrorMessage = "Policy Id is required")]
    public int PolicyId { get; set; }
    
    [Required(ErrorMessage = "Market Value is required")]
    public double MarketValue { get; set; }
}
