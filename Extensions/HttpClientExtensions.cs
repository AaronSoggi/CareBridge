using System.Net.Http.Headers;
using MediApp.Configuration;
using MediApp.Services;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.Extensions.Options;
using Polly.Extensions.Http;
using Polly;
using System.Net;
using System.Threading.Tasks;

namespace MediApp.Extensions;

public static class HttpClientExtensions
{
    public static IServiceCollection AddHttpClients(IServiceCollection services, IConfiguration config)
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

        return services;
    }

    public static IAsyncPolicy<HttpResponseMessage> AddRetryPolicy(ILogger logger)
    {
        return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(t => t.StatusCode == HttpStatusCode.TooManyRequests)
        .WaitAndRetryAsync(3, sleepDurationProvider: 
        attempt => 
        TimeSpan.FromSeconds(Math.Pow(2, attempt)), 
        onRetry: (result, timespan, retryCount, context) =>
        {
            var reason = result.Exception?.Message ?? result.Result.StatusCode.ToString() ?? "Unknown issue";

            logger.LogWarning($"Retry number {retryCount}, was delayed by: {timespan}. Reason: {reason}: Context: {context.OperationKey}");
        });
    }

    public static IAsyncPolicy<HttpResponseMessage> CircuitBreakerAsync(ILogger logger)
    {
        return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(t => t.StatusCode == HttpStatusCode.TooManyRequests)
        .CircuitBreakerAsync(handledEventsAllowedBeforeBreaking: 3, 
        durationOfBreak: TimeSpan.FromSeconds(30), 
        onBreak: (outcome, duration, context) =>
        {
            var reason = outcome.Exception.Message ?? outcome.Result.StatusCode.ToString() ?? "unknown";

            logger.LogWarning("Cicuit has reached the maxmimum limit and will be open for {duration}. Reason for cause: {reason} - Context : {context}",
            duration,
            reason,
            context.OperationKey);
            
        },onReset: context =>
        {
            logger.LogWarning("Circuit has now closed and is taking further requests {context}", context);
            
        }, onHalfOpen: () =>
        {
            logger.LogInformation("Circuit is going through a testing process to see how requests are being handled");
        });
    }

    public static IAsyncPolicy<HttpResponseMessage> GeneratePolicies(ILogger logger)
    {
        var retry = AddRetryPolicy(logger);
        var circuit = CircuitBreakerAsync(logger);

        var policy = Policy.Handle<HttpRequestException>()
        .OrResult<HttpResponseMessage>(t => t.StatusCode == HttpStatusCode.TooManyRequests)
        .WaitAndRetryAsync(retryCount: 3, sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(3, attempt)));


        return Policy.WrapAsync(circuit, retry);
    }
}