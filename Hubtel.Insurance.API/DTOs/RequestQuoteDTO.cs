namespace Hubtel.Insurance.API.DTOs;

using System.ComponentModel.DataAnnotations;

public class RequestQuoteDTO
{
    public int PolicyId { get; set; }
    
    public double MarketValue { get; set; }
}
