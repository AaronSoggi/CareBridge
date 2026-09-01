using MediApp.DTOs;
using System.Net;
using System.Text;
using System.Text.Json;

namespace MediApp.Services;

public class ProductClient : IProductClient
{
    private readonly HttpClient _client;
    public ProductClient(HttpClient client)
    {
        _client = client;   
    }

    public async Task<ProductDto> GetAsync(int id)
    {
        var response = await _client.GetFromJsonAsync<ProductDto>($"/products/{id}");

        return response ?? throw new Exception("no data returned");
    }

    public async Task<UpdateProductDto> UpdateAsync(int id)
    {
        var updatedProduct = new UpdateProductDto
        {
            Title = "title test",
            Description = "test description",
            Category = "testing",
        };

        var json = JsonSerializer.Serialize(updatedProduct);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PutAsync($"products/{id}", content);

        response.EnsureSuccessStatusCode();

        var data = await response.Content.ReadAsStringAsync();
        var dto = JsonSerializer.Deserialize<UpdateProductDto>(data);

        return dto ?? throw new Exception("No data returned");
    }

    public async Task<CreateProductDto> CreateProductAsync (CreateProductDto dto)
    {

        var settings = new JsonSerializerOptions
        {
           WriteIndented = false,
           PropertyNameCaseInsensitive = true,  
        };

        var json = JsonSerializer.Serialize(dto, settings);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _client.PostAsync("products/add", content);

        if(!response.IsSuccessStatusCode)
        {
            if(response.StatusCode == HttpStatusCode.Forbidden)
            {
                var errors = await response.Content.ReadAsStringAsync();
                //_logger.LogWarning($"Please ensure you have the correct permissions on your account before carrying out this request: {errors}");

            }
        }

        var format = await response.Content.ReadAsStringAsync();
        var jsonData = JsonSerializer.Deserialize<CreateProductDto>(format);

        return jsonData ?? throw new Exception("No data was returned");
        
    }
}