using CryptoQuotes.Controllers;
using CryptoQuotes.Models;
using CryptoQuotes.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CryptoQuotesTest.Controllers;

public class CryptocurrencyControllerTest
{
    private readonly Mock<ICoinMarketCapClient> _coinMarketCapClient = new();
    private readonly CryptocurrencyController _cryptocurrencyController;

    public CryptocurrencyControllerTest()
    {
        _cryptocurrencyController = new CryptocurrencyController(_coinMarketCapClient.Object);
    }

    [Fact]
    public async Task GetCryptoQuotes_WithValidResponse_ReturnsListOfQuotes()
    {
        _coinMarketCapClient.Setup(x => x.GetQuotesForCryptocurrency(It.IsAny<string>()))
            .ReturnsAsync(GetValidResponse());

        var actionResult = await _cryptocurrencyController.GetCryptoQuotes("BTC");
        
        Assert.Equal(typeof(OkObjectResult), actionResult.Result!.GetType());
        
        var actualResult = (actionResult.Result as OkObjectResult)!.Value! as Dictionary<string, CoinMarketCapData>;
        
        Assert.Single(actualResult!.Keys);
        Assert.Equal("BTC", actualResult.Keys.First());
        Assert.Equal("BTC", actualResult.GetValueOrDefault("BTC")!.Symbol);
        Assert.Equal(5, actualResult.GetValueOrDefault("BTC")!.Quote.Count);
    }

    [Fact]
    public async Task GetCryptoQuotes_WithNonExistingCryptoCode_ReturnsBadRequestWithCodeError()
    {
        _coinMarketCapClient.Setup(x => x.GetQuotesForCryptocurrency(It.IsAny<string>()))
            .ReturnsAsync((CoinMarketCapResponse)null!);

        var actionResult = await _cryptocurrencyController.GetCryptoQuotes("BTCDOGE");
        
        Assert.Equal(typeof(BadRequestObjectResult), actionResult.Result!.GetType());
        Assert.Equal("The provided cryptocurrency code does not exist.", (actionResult.Result! as BadRequestObjectResult)!.Value);
    }

    [Fact]
    public async Task GetCryptoQuotes_WithErrorRequest_ReturnsBadRequestWithProperErrorMessage()
    {
        _coinMarketCapClient.Setup(x => x.GetQuotesForCryptocurrency(It.IsAny<string>()))
            .ReturnsAsync(GetErrorResponse());

        var actionResult = await _cryptocurrencyController.GetCryptoQuotes("BTC");
        
        Assert.Equal(typeof(BadRequestObjectResult), actionResult.Result!.GetType());
        Assert.Equal("Your plan is limited to 1 convert options", (actionResult.Result! as BadRequestObjectResult)!.Value);
    }

    private CoinMarketCapResponse GetValidResponse()
    {
        return new CoinMarketCapResponse
        {
            Status = new CoinMarketCapStatus
            {
                ErrorCode = 0
            },
            Data = new Dictionary<string, CoinMarketCapData>
            {
                {
                    "BTC", new CoinMarketCapData
                    {
                        Id = 1,
                        Name = "Bitcoin",
                        Symbol = "BTC",
                        Quote = new Dictionary<string, CoinMarketCapQuote>
                        {
                            { "USD", new CoinMarketCapQuote { Price = "1000" } },
                            { "EUR", new CoinMarketCapQuote { Price = "1000" } },
                            { "BRL", new CoinMarketCapQuote { Price = "1000" } },
                            { "GBP", new CoinMarketCapQuote { Price = "1000" } },
                            { "AUD", new CoinMarketCapQuote { Price = "1000" } }
                        }
                    }
                }
            }
        };
    }

    private CoinMarketCapResponse GetErrorResponse()
    {
        return new CoinMarketCapResponse
        {
            Status = new CoinMarketCapStatus
            {
                ErrorCode = 400,
                ErrorMessage = "Your plan is limited to 1 convert options"
            },
            Data = new Dictionary<string, CoinMarketCapData>()
        };
    }
}