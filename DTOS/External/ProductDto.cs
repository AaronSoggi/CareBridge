namespace MediApp.DTOs;

public class ProductDto
{
    public int Id {get;set;}
    public string Title {get;set;}
    public string Description {get;set;}
    public string Category {get;set;}
    public decimal Price {get;set;}
    public decimal DiscountPercentage {get;set;}
    public decimal Rating {get;set;}
    public List<string> Tags {get;set;} = [];
}