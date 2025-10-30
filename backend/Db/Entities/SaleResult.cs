using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.Identity.Client;
using Microsoft.Net.Http.Headers;

namespace backend.Db.Entities;

public class SaleResult
{
    public Guid Id { get; set; }
    
    public Guid AuctionItemId { get; set; }
     public Guid BuyerId { get; set; }
    public decimal FinalPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalProceeds { get; set; }
}