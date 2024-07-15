using System.Threading.Tasks;
using CryptoQuotes.Models;

namespace CryptoQuotes.Services;

public interface ICoinMarketCapClient
{
    /// <summary>
    /// Get the quotes in different currencies for a specific cryptocurrency
    /// </summary>
    /// <param name="cryptoCode">The code of the cryptocurrency from which to get the quotes</param>
    /// <returns>An object containing the status of the response of the API and the data for the quotes.
    /// Null if the cryptocurrency code does not correspond to a real cryptocurrency</returns>
    Task<CoinMarketCapResponse?> GetQuotesForCryptocurrency(string cryptoCode);
}