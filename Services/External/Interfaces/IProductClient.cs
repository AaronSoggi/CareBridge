using MediApp.DTOs;
namespace MediApp.Services;


public interface IProductClient
{
     Task<ProductDto> GetAsync(int id);
     Task<UpdateProductDto> UpdateAsync(int id);
     Task<CreateProductDto> CreateProductAsync (CreateProductDto dto);
}