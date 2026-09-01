using System.Net.Http.Headers;
using MediApp.Configuration;
using MediApp.Services;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.Extensions.Options;
using Polly.Extensions.Http;
using Polly;
using System.Net;
using System.Threading.Tasks;
using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using MediApp.Models;
using Microsoft.CodeAnalysis.Options;

namespace MediApp.Extensions;

public static class HttpClientExtensions
{
    public static IServiceCollection AddHttpClients(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<MedicationClientSettings>(config.GetSection("HttpClients:MedicationApi"));

        services.AddHttpClient<IMedicationClient, MedicationClient>((sg, client) =>
        {
            var config = sg.GetRequiredService<IOptions<MedicationClientSettings>>();
            client.BaseAddress = new Uri(config.Value.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(config.Value.TimeoutSeconds);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }).AddPolicyHandler((sp,request) =>
        {
            var logger = sp.GetRequiredService<ILogger<MedicationClient>>();
            return GeneratePolicies(logger);

        });

        //Product client
        services.Configure<ProductClientSettings>(config.GetSection("HttpClients:ProductApi"));

        services.AddHttpClient<IProductClient, ProductClient>((sg, client) =>
        {
           var config = sg.GetRequiredService<IOptions<ProductClientSettings>>().Value;
           client.BaseAddress = new Uri(config.BaseUrl);
           client.Timeout = TimeSpan.FromSeconds(config.Timeout);
           client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }).AddPolicyHandler((sp, request) =>
        {
            var logger = sp.GetRequiredService<ILogger<ProductClient>>();
            return WaitAndRetryPolicy(logger);
        });

        services.Configure<MedicationClientSettings>(config.GetSection("HttpClients:MedicationApi"));

        services.AddHttpClient<IMedicationClient, MedicationClient>((sg, client) =>
        {
            var service = sg.GetRequiredService<IOptions<MedicationClientSettings>>().Value;
            client.BaseAddress = new Uri(service.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(service.TimeoutSeconds);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application.json"));
        }).AddPolicyHandler((sp, client) =>
        {
            var logger = sp.GetRequiredService<ILogger<MedicationClient>>();
            return WaitAndRetryPolicy(logger);
        });
        
        return services;
    }
    
    public static IAsyncPolicy<HttpResponseMessage> WaitAndRetryPolicy(ILogger logger)
    {
        return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(t => t.StatusCode == HttpStatusCode.TooManyRequests)
        .WaitAndRetryAsync(retryCount: 3, sleepDurationProvider: attempt 
        => TimeSpan.FromSeconds(Math.Pow(2, attempt)), onRetry: (result, duration, attempt, context) =>
        {
            var outcome = result.Exception.Message ?? result.Result.StatusCode.ToString() ?? "unknown";

            logger.LogWarning($"Response {outcome}, sleep duration: {duration}, on attempt {attempt}: Operation Key {context.OperationKey} ");   
        });
    }

    public static IAsyncPolicy<HttpResponseMessage> AddRetryPolicy(ILogger logger)
    {
        return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(t => t.StatusCode == HttpStatusCode.TooManyRequests)
        .WaitAndRetryAsync(retryCount: 3, sleepDurationProvider: attempt =>
        TimeSpan.FromSeconds(Math.Pow(2, attempt)), onRetry: (result,duration, attempt, context) =>
        {
            var outcome = result.Exception.Message ?? result.Result.StatusCode.ToString() ?? "unknown";

            logger.LogWarning($"Response {outcome}, sleep duration: {duration}, on attempt {attempt}: Operation Key {context.OperationKey} ");   

        });
    }

    public static IAsyncPolicy<HttpResponseMessage> CircuitBreakerAsync(ILogger logger)
    {
        return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(i => i.StatusCode == HttpStatusCode.TooManyRequests)
        .CircuitBreakerAsync(handledEventsAllowedBeforeBreaking: 3, 
        durationOfBreak: TimeSpan.FromSeconds(30),
        onBreak: (result,duration,context) =>
        {
            var outcome = result.Exception.Message ?? result.Result.StatusCode.ToString() ?? "unknown";
            logger.LogWarning("Circuit is now taking a break for {duration}, as failed to meet expected standards: Reason: {outcome} : operation key {context}", 
            duration, 
            outcome,
            context.OperationKey);
        }, 
        onReset: context => 
        {
            logger.LogWarning("Circuit has now closed and is taking further requests {context}",context);
            
        }, onHalfOpen: () =>
        {
            logger.LogWarning("Circuit is going through a testing process to determine how requests are being handled.");
        });
    }

    public static IAsyncPolicy<HttpResponseMessage> GeneratePolicies(ILogger logger)
    {
        var retry = AddRetryPolicy(logger);
        var circuit = CircuitBreakerAsync(logger);

        return Policy.WrapAsync(circuit, retry);
    }
}