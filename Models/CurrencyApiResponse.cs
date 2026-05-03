using System.Text.Json.Serialization;

namespace CurrencyApp.Models;

public class CurrencyApiResponse
{
    [JsonPropertyName("Date")]
    public DateTime Date { get; set; }

    [JsonPropertyName("PreviousDate")]
    public DateTime PreviousDate { get; set; }

    [JsonPropertyName("Timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("Valute")]
    public Dictionary<string, Currency> Valute { get; set; } = new();
}