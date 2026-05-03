using System.Text.Json.Serialization;

namespace CurrencyApp.Models;

public class Currency
{
    [JsonPropertyName("ID")]
    public string? Id { get; set; }

    [JsonPropertyName("NumCode")]
    public string? NumCode { get; set; }

    [JsonPropertyName("CharCode")]
    public string? CharCode { get; set; }

    [JsonPropertyName("Nominal")]
    public int Nominal { get; set; }

    [JsonPropertyName("Name")]
    public string? Name { get; set; }

    [JsonPropertyName("Value")]
    public decimal Value { get; set; }

    [JsonPropertyName("Previous")]
    public decimal Previous { get; set; }

    // Not from API — marks currencies the user added manually
    // so refresh from the API doesn't overwrite them
    [JsonIgnore]
    public bool IsUserAdded { get; set; }
}