using System.Net;
using System.Text.Json;
using CryptoQuotes.Models;
using CryptoQuotes.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;

namespace CryptoQuotesTest.Services;

public class CoinMarketCapClientTests
{
    private readonly IConfiguration _configuration;

    public CoinMarketCapClientTests()
    {
        var inMemorySettings = new Dictionary<string, string>
        {
            { "CoinMarketCapApi:Address", "http://localhost" },
            { "CoinMarketCapApi:ApiKey", "api_key_secret" },
            { "CoinMarketCapApi:Currencies", "USD,EUR,BRL,GBP,AUD" }
        };
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();
    }

    [Fact]
    public async Task GetQuotesForCryptocurrency_RetrievesCorrectData()
    {
        var expectedResult = new CoinMarketCapResponse
        {
            Status = new CoinMarketCapStatus
            {
                ErrorCode = 0
            },
            Data = new Dictionary<string, CoinMarketCapData>
            {
                { "BTC", new CoinMarketCapData
                    {
                        Id = 1,
                        Name = "Bitcoin",
                        Symbol = "BTC",
                        Quote = new Dictionary<string, CoinMarketCapQuote>
                        {
                            {"USD", new CoinMarketCapQuote{ Price = "1000" }},
                            {"EUR", new CoinMarketCapQuote{ Price = "1000" }},
                            {"BRL", new CoinMarketCapQuote{ Price = "1000" }},
                            {"GBP", new CoinMarketCapQuote{ Price = "1000" }},
                            {"AUD", new CoinMarketCapQuote{ Price = "1000" }}
                        }
                    }
                }
            }
        };

        var httpMessageHandler = new Mock<HttpMessageHandler>();
        var httpHandler = httpMessageHandler.Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );

        foreach (var currency in _configuration["CoinMarketCapApi:Currencies"]!.Split(","))
        {
            httpHandler = AddReturn(httpHandler, currency, "1000");
        }

        var httpClient = new HttpClient(httpMessageHandler.Object)
        {
            BaseAddress = new Uri(_configuration["CoinMarketCapApi:Address"]!)
        };

        var httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory.Setup(x => x.CreateClient("CoinMarketCap")).Returns(httpClient);

        var coinMarketCapClient = new CoinMarketCapClient(httpClientFactory.Object, _configuration);

        var actualResult = await coinMarketCapClient.GetQuotesForCryptocurrency("BTC");

        Assert.Single(actualResult!.Data);
        Assert.Equal(expectedResult.Data.Count, actualResult!.Data.Count);
        Assert.Equal(expectedResult.Data.GetValueOrDefault(expectedResult.Data.Keys.First())!.Quote.Count,
            actualResult.Data.GetValueOrDefault(actualResult.Data.Keys.First())!.Quote.Count);
    }

    [Fact]
    public async Task GetQuotesForCryptocurrency_IfCryptocodeDoesNotExist_ReturnsNull()
    {
        var expectedResult = new CoinMarketCapResponse
        {
            Status = new CoinMarketCapStatus
            {
                ErrorCode = 0
            },
            Data = new Dictionary<string, CoinMarketCapData>()
        };
        var httpMessageHandler = new Mock<HttpMessageHandler>();
        var httpHandler = httpMessageHandler.Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(expectedResult))
            });

        foreach (var currency in _configuration["CoinMarketCapApi:Currencies"]!.Split(","))
        {
            httpHandler = AddReturn(httpHandler, currency, "1000");
        }

        var httpClient = new HttpClient(httpMessageHandler.Object)
        {
            BaseAddress = new Uri(_configuration["CoinMarketCapApi:Address"]!)
        };

        var httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory.Setup(x => x.CreateClient("CoinMarketCap")).Returns(httpClient);

        var coinMarketCapClient = new CoinMarketCapClient(httpClientFactory.Object, _configuration);

        var actualResult = await coinMarketCapClient.GetQuotesForCryptocurrency("BTC");

        Assert.Null(actualResult);
    }

    private CoinMarketCapResponse GenerateResponse(string currency, string price)
    {
        return new CoinMarketCapResponse
        {
            Status = new CoinMarketCapStatus
            {
                ErrorCode = 0
            },
            Data = new Dictionary<string, CoinMarketCapData>
            {
                { "BTC", new CoinMarketCapData
                    {
                        Id = 1,
                        Name = "Bitcoin",
                        Symbol = "BTC",
                        Quote = new Dictionary<string, CoinMarketCapQuote>
                        {
                            {currency, new CoinMarketCapQuote{ Price = price }}
                        }
                    }
                }
            }
        };
    }

    private Moq.Language.ISetupSequentialResult<Task<HttpResponseMessage>> AddReturn(
        Moq.Language.ISetupSequentialResult<Task<HttpResponseMessage>> handler,
        string currency, string price)

    {
        return handler
            .ReturnsAsync(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(GenerateResponse(currency, price)))
            });
    }
}