


using System.Net;
using System.Net.Http.Headers;
using MediApp.Configuration;
using MediApp.Services;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;

namespace MediApp.Extensions;

public static class HttpClientPractice
{
    public static IServiceCollection CreateClients(this IServiceCollection service, IConfiguration config)
    {
        service.Configure<MedicationClientSettings>(config.GetSection("HttpClient:MedicationClient"));

        service.AddHttpClient<IMedicationClient, MedicationClient>((sp, client) =>
        {
            var config = sp.GetRequiredService<IOptions<MedicationClientSettings>>().Value;
            client.BaseAddress = new Uri(config.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }).AddPolicyHandler((sp, request) =>
        {
            var logger = sp.GetRequiredService<ILogger<MedicationClient>>();
            return CombiningPolicies(logger);
            
        });

        return service;
    }

    public static IAsyncPolicy<HttpResponseMessage> WaitAndRetry(ILogger logger)
    {
        return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(i => i.StatusCode == HttpStatusCode.TooManyRequests)
        .WaitAndRetryAsync(retryCount: 3, sleepDurationProvider: attempt => 
        TimeSpan.FromSeconds(Math.Pow(2, attempt)), 
        onRetry: (result,duration, retryCount, context) =>
        {
            var outcome = result.Exception.Message ?? result.Result.StatusCode.ToString() ?? "Unknown";

            logger.LogWarning("retry count number: {retryCouny}, sleep duration: {duration}, Operation key: {context}"
            ,retryCount,duration, context.OperationKey);
        });
    }
    
    public static IAsyncPolicy<HttpResponseMessage> CircuitBreaker(ILogger logger)
    {
        return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(i => i.StatusCode == HttpStatusCode.TooManyRequests)
        .CircuitBreakerAsync(handledEventsAllowedBeforeBreaking: 3, durationOfBreak: TimeSpan.FromSeconds(30)
        ,onBreak:(result, duration, context) =>
        {
            var outcome = result.Exception.Message ?? result.Result.StatusCode.ToString() ?? "Unknown";

            logger.LogWarning("Will wait for {duration} before circuit closes, Operation Key: {context}"
            ,duration, context.OperationKey);
            
        }, onReset: context =>
        {
            logger.LogWarning("Circuit has now closed and isn taking further requests {context}", context.OperationKey);
            
        },onHalfOpen: () =>
        {
            logger.LogWarning("Testing to see if server is accepting any further requests");
        });
    }

    public static IAsyncPolicy<HttpResponseMessage> CombiningPolicies(ILogger logger)
    {
        var circuit = CircuitBreaker(logger);
        var waitAndRetry = WaitAndRetry(logger);

        return Policy.WrapAsync(circuit, waitAndRetry);
        
    }
    
}
