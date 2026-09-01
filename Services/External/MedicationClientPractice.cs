using System.Net;
using System.Text;
using System.Text.Json;
using Azure;
using MediApp.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;

namespace MediApp.Services.External;

public class MedicationClientPractice
{
    private readonly IMedicationClient _medicationClient;
    private readonly HttpClient _httpclient;

    private readonly ILogger<MedicationClientPractice> _logger;

    public MedicationClientPractice(IMedicationClient medicationClient, HttpClient httpClient, ILogger<MedicationClientPractice> logger)
    {
        _medicationClient = medicationClient;
        _httpclient = httpClient;
        _logger = logger;
    }
    public async Task<Medication> PostAsync()
    {
        var medication = new Medication
        {
            Name = "ibuprofen",
            Dose = 2,
            Instructions = "take one a day",
            StartDate = DateTime.UtcNow,
            EndDate = new DateTime(DateTime.DaysInMonth(2026, 03))

        };

        var json = JsonConvert.SerializeObject(medication, formatting: Formatting.None);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage responseMessage = await _httpclient.PostAsync("data", content);

        if(!responseMessage.IsSuccessStatusCode)
        {
            switch (responseMessage.StatusCode)
            {
                case HttpStatusCode.Forbidden:
                _logger.LogWarning("User is not authorised to create this medication");
                return null;

                case HttpStatusCode.ServiceUnavailable:
                _logger.LogWarning("The medication is unavailable at this time {Api}", _httpclient.BaseAddress);
                return null;
            }
            
        }

        var jsonData = await responseMessage.Content.ReadAsStringAsync();
        var med = JsonConvert.DeserializeObject<Medication>(jsonData);

        return med ?? throw new Exception("no data was returned");
    }

    public async Task<Medication> GetAsync()
    {
        HttpResponseMessage response = await _httpclient.GetAsync("/data");

        response.EnsureSuccessStatusCode();

       var obj = await response.Content.ReadFromJsonAsync<Medication>();

       return obj ?? throw new Exception("unable to return object");
    }

    public async Task<Medication> SendAsync()
    {
        var request  = new HttpRequestMessage(HttpMethod.Post, "www.google.com");
        request.Headers.Add("Authorization", "Your_Bearer_token");

        var obj = new Medication
        {
            Name = "ibuprofen",
            Dose = 2,
            Instructions = "take one a day",
            StartDate = DateTime.UtcNow,
            EndDate = new DateTime(DateTime.DaysInMonth(2026, 03))
        };

        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        var json = JsonConvert.SerializeObject(obj, settings);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpclient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("something went wrong duirng the post request");
        }

        var result = await response.Content.ReadFromJsonAsync<Medication>();

        return result ?? throw new Exception("no data was returned");
    }

    public async Task<Medication> GetFromJsonAsync()
    {
        var response = await _httpclient.GetFromJsonAsync<Medication>("localhost2355/medication/1");

        return response ?? throw new Exception("no data was returned");

    }

    public async Task<Medication> PostAsJsonAsync()
    {

        var med = new Medication
        {
            Name = "ibuprofen",
            Dose = 2,
            Instructions = "take one a day",
            StartDate = DateTime.UtcNow,
            EndDate = new DateTime(DateTime.DaysInMonth(2026, 03))
        };

        var response = await _httpclient.PostAsJsonAsync<Medication>("/data", med);

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        var obj = JsonConvert.DeserializeObject<Medication>(content);

        return obj ?? throw new Exception("No data was returned");
    }

    public async Task<Medication> PutAsync(int id)
    {

        var med = new Medication
        {
            Name = "ibuprofen",
            Dose = 2,
            Instructions = "take one a day",
            StartDate = DateTime.UtcNow,
            EndDate = new DateTime(DateTime.DaysInMonth(2026, 03))
        };

        var json = JsonConvert.SerializeObject(med);
        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _httpclient.PutAsync($"http://localhost:5088/api/medication/{id}", httpContent);

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        var obj = JsonConvert.DeserializeObject<Medication>(content);

        return obj ?? throw new Exception("no data was returned");
        
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
    //be able to explain when to use each
}