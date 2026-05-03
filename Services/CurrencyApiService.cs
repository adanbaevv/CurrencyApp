using System.Net.Http;
using System.Net.Http.Json;
using CurrencyApp.Models;

namespace CurrencyApp.Services;

public class CurrencyApiService
{
    private const string ApiUrl = "https://www.cbr-xml-daily.ru/daily_json.js";

    private readonly HttpClient _httpClient;

    public CurrencyApiService()
    {
        _httpClient = new HttpClient();
    }

    public async Task<List<Currency>> FetchCurrenciesAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<CurrencyApiResponse>(ApiUrl);

        if (response?.Valute == null)
        {
            return new List<Currency>();
        }

        // Convert the dictionary to a flat list — easier for the UI to bind to
        return response.Valute.Values.ToList();
    }
}