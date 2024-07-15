using System.Threading.Tasks;
using CryptoQuotes.Models;
using CryptoQuotes.Services;
using Microsoft.AspNetCore.Mvc;

namespace CryptoQuotes.Controllers;

[ApiController]
public class CryptocurrencyController(ICoinMarketCapClient coinMarketCapClient) : ControllerBase
{
    [HttpGet("{cryptoCode}")]
    public async Task<ActionResult<CoinMarketCapData>> GetCryptoQuotes(string cryptoCode)
    {
        var response = await coinMarketCapClient.GetQuotesForCryptocurrency(cryptoCode);
        
        // If the returned object is null, it means the cryptocurrency code does not exist
        if (response == null)
        {
            return BadRequest("The provided cryptocurrency code does not exist.");
        }
        
        if (response.Status.ErrorCode == 0)
        {
            return Ok(response.Data);
        }

        return BadRequest(response.Status.ErrorMessage);
    }
}