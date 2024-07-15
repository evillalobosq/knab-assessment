using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CryptoQuotes.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace CryptoQuotes.Services;

public class CoinMarketCapClient : ICoinMarketCapClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly List<string> _currencies;
    
    public CoinMarketCapClient(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClient = httpClientFactory.CreateClient("CoinMarketCap");
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient.DefaultRequestHeaders.Add("X-CMC_PRO_API_KEY", configuration["CoinMarketCapApi:ApiKey"]);
        _baseUrl = configuration["CoinMarketCapApi:Address"]!;
        _currencies = configuration["CoinMarketCapApi:Currencies"]!.Split(',').ToList();
    }
    
    public async Task<CoinMarketCapResponse?> GetQuotesForCryptocurrency(string cryptoCode)
    {
        // Initiate the object as null, that way if the returned object is null, it means we broke out of the iteration because
        // the cryptocurrency code does not exist.
        CoinMarketCapResponse? quotes = null;
        
        foreach (var currency in _currencies)
        {
            var data = await GetCurrencyQuote(cryptoCode, currency);
            // If there is no error, we continue with the response, otherwise we move on to the next one.
            if (data is null || data.Status.ErrorCode != 0) continue;
            
            // If the cryptocurrency code doesn't exist, the Data object contains 0 elements, so we break away from the
            // iteration to avoid further unnecessary http calls
            if (data.Data.Count == 0)
            {
                break;
            }
                
            // We fill the object with the element we get from the first call
            if (quotes == null)
            {
                quotes = data;
            }
                
            // And the consecutive calls only add information to the quotes dictionary.
            else
            {
                var key = data.Data.GetValueOrDefault(cryptoCode)?.Quote.Keys.First()!;
                var value = data.Data.GetValueOrDefault(cryptoCode)?.Quote[key]!;
                quotes.Data.GetValueOrDefault(cryptoCode)?.Quote.Add(key, value);
            }
        }

        return quotes;
    }

    private async Task<CoinMarketCapResponse?> GetCurrencyQuote(string cryptoCode, string currency)
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}?symbol={cryptoCode}&convert={currency}");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var quoteResponse = JsonConvert.DeserializeObject<CoinMarketCapResponse>(content);

        return quoteResponse;
    }
}