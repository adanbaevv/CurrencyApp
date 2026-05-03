using System.IO;
using System.Text.Json;
using CurrencyApp.Models;

namespace CurrencyApp.Services;

public class JsonStorageService : IStorageService
{
    private readonly string _filePath;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public JsonStorageService()
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "CurrencyApp"
        );

        Directory.CreateDirectory(folder);

        _filePath = Path.Combine(folder, "currencies.json");
    }

    public async Task<List<Currency>> LoadAsync()
    {
        if (!File.Exists(_filePath))
        {
            return new List<Currency>();
        }

        await using var stream = File.OpenRead(_filePath);
        var currencies = await JsonSerializer.DeserializeAsync<List<Currency>>(stream, JsonOptions);
        return currencies ?? new List<Currency>();
    }

    public async Task SaveAsync(List<Currency> currencies)
    {
        await using var stream = File.Create(_filePath);
        await JsonSerializer.SerializeAsync(stream, currencies, JsonOptions);
    }
}