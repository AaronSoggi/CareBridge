namespace MediApp.DTOs;

public class CreateProductDto
{
    public string Title {get;set;}
    public string Description {get;set;}
    public string Category {get;set;}
    public decimal Price {get;set;}
    public decimal DiscountPercentage {get;set;}
    public decimal Rating {get;set;}

}