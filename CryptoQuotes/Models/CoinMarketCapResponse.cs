using System.Collections.Generic;

namespace CryptoQuotes.Models;

public class CoinMarketCapResponse
{
    public CoinMarketCapStatus Status { get; set; } = null!;
    public Dictionary<string, CoinMarketCapData> Data { get; set; } = null!;
}

public class CoinMarketCapStatus
{
    public int ErrorCode { get; set; }
    public string ErrorMessage { get; set; } = null!;
}

public class CoinMarketCapData
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Symbol { get; set; } = null!;
    public Dictionary<string, CoinMarketCapQuote> Quote { get; set; } = null!;
}

public class CoinMarketCapQuote
{
    public string Price { get; set; } = null!;
}