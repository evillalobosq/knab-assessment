using System;
using System.Net.Http;
using CryptoQuotes.Services;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace CryptoQuotes.Configurations;

public static class Services
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services)
    {
        services.AddHttpClient()
            .AddTransient<ICoinMarketCapClient, CoinMarketCapClient>();

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(2 * retryAttempt));
    }

    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(15));
    }
}