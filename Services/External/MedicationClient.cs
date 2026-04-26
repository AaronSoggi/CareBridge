using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.VisualBasic;


namespace MediApp.Services;

public class MedicationClient : IMedicationClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<MedicationClient> _logger;
    public MedicationClient(IHttpClientFactory httpClientFactory, ILogger<MedicationClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger= logger;
    }

    public async Task<Medication> GetMedicationAsync()
    {
        var client = _httpClientFactory.CreateClient();

        var response = await client.GetFromJsonAsync<Medication>("http://www.google.com");

        return response ?? throw new Exception("No data was returned");
    }

    public async Task<Medication> MedicationPostJsonAsync()
    {
        var client = _httpClientFactory.CreateClient();

        var medication = new Medication
        {
            Name = "ibuprofen",
            Dose = "200"  
        };

        HttpResponseMessage response = await client.PostAsJsonAsync("http://localhost:5088/api/medication", medication);

        response.EnsureSuccessStatusCode();

        var obj = await response.Content.ReadFromJsonAsync<Medication>();
        return obj ?? throw new Exception("no data returned");
    }

    public async Task<Medication> MedicationPutAsync(int id)
    {
        var client = _httpClientFactory.CreateClient();

        var medication = new Medication
        {
            Name = "ibuprofen",
            Dose = "200"
        };

        var response = await client.PutAsJsonAsync($"http://localhost:5088/api/medication/{id}", medication);

        response.EnsureSuccessStatusCode();

        var obj = await response.Content.ReadFromJsonAsync<Medication>();
        return obj ?? throw new Exception("No data returned");

    } 

    public async Task MedicationDeleteAsync(int id)
    {
        var client = _httpClientFactory.CreateClient();

        HttpResponseMessage response = await client.DeleteAsync($"http://localhost:5088/api/medication/{id}");

        response.EnsureSuccessStatusCode();
    }

    public async Task<Object> MedicationPostAsync()
    {
        var client = _httpClientFactory.CreateClient();

        var medication = new Medication
        {
            Name = "ibuprofen",
            Dose = "200"
        };

        var settings = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        var json = JsonSerializer.Serialize(medication, settings);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await client.PostAsync("/data", content);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("something went wrong when creating a medication");
            throw new Exception("no data was returned");
        }

        var jsonData = await response.Content.ReadAsStringAsync();
        var obj = JsonSerializer.Deserialize<Medication>(jsonData);

        return obj ?? throw new Exception("No data returned");
    }

    public async Task<Medication> GetAsync(int id)
    {
        var client = _httpClientFactory.CreateClient();

        var response = await client.GetAsync($"http://localhost:5088/api/medication/{id}");

        response.EnsureSuccessStatusCode();

        var obj = await response.Content.ReadFromJsonAsync<Medication>();
        return obj ?? throw new Exception("No data returned");
    }

    public async Task<Medication> MedicationSendAsync()
    {
        var client = _httpClientFactory.CreateClient();

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:5088/api/medication");

        request.Headers.Add("Authorization", "Bearer your_access_token");

        var medication = new Medication
        {
            Name = "paracetamol",
            Dose = "300"
        };

        var json = JsonSerializer.Serialize(medication);
        _logger.LogInformation("Medication sent up in request: {json}", json);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Something went wrong when attempting to create a medication");
            if(response.StatusCode == HttpStatusCode.BadRequest)
            {
                var errors = response.Content.ReadAsStringAsync();
                _logger.LogWarning($"failed to create a mediction: {errors}");
            }
            response.EnsureSuccessStatusCode();
        }

        var data = await response.Content.ReadFromJsonAsync<Medication>();
        return data ?? throw new Exception("no data returned");
    }



    //reusable sendAsync method
    public async Task<TResponse> MedicationReusableSendAsync<TRequest, TResponse>(string url, TRequest body, 
    Dictionary<string, string> headers)
    {
        var client = _httpClientFactory.CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Content = JsonContent.Create(body);

        if(headers != null)
        {
            foreach(var header in headers)
            {
                request.Headers.Add(header.Key, header.Value);
            }
        }

        var response = await client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            if(response.StatusCode == HttpStatusCode.Forbidden)
            {
                var error = response.Content.ReadAsStringAsync();
                _logger.LogWarning("You dont  have thr required persmissions to carry our this request {error}", error);
            }

            response.EnsureSuccessStatusCode();
        }

        var obj = await response.Content.ReadFromJsonAsync<TResponse>();

        return obj ?? throw new Exception("no data returned");
        
    }

    public async Task<T> GetMedication<T>(T obj, string url)
    {
        var client = _httpClientFactory.CreateClient();

        var data = await client.GetFromJsonAsync<T>(url);

        return data ?? throw new Exception("no data returned");

    }
    //GetFromJsonAsync
    //ReadFromJsonAsync = reading response content as string and deserializing.
    //EnsureSuccessStatusCode
    //PostAsJsonAsync
    //PutAsJsonAsync
    //DeleteAsync
    //PostAsync
    //GetAsync
    //SendAsync
    //Make a reusable sendAsync
    //Adding retry logic with polly
    // be able to explain when to use each

    public class Medication()
    {
        [JsonPropertyName("med_name")]
        public string Name {get;set;}
        [JsonPropertyName("med_dose")]
        public string Dose {get;set;}
    }
}