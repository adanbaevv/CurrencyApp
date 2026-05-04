using CurrencyApp.Models;

namespace CurrencyApp.Services;

public enum StorageMode
{
    Json,
    Sqlite,
    Both
}

public class CompositeStorageService : IStorageService
{
    private readonly JsonStorageService _json;
    private readonly SqliteStorageService _sqlite;
    private readonly StorageMode _mode;

    public CompositeStorageService(StorageMode mode)
    {
        _json = new JsonStorageService();
        _sqlite = new SqliteStorageService();
        _mode = mode;
    }

    //public async Task<List<Currency>> LoadAsync()
    //{
    //    // Read from primary backend. In Both mode, JSON is the read source —
    //    // both stores are kept in sync by writes, so either would work.
    //    return _mode switch
    //    {
    //        StorageMode.Sqlite => await _sqlite.LoadAsync(),
    //        _ => await _json.LoadAsync()
    //    };
    //}

    public async Task<List<Currency>> LoadAsync()
    {
        return _mode switch
        {
            StorageMode.Json => await _json.LoadAsync(),
            StorageMode.Sqlite => await _sqlite.LoadAsync(),
            StorageMode.Both => await LoadFromBothAsync(),
            _ => new List<Currency>()
        };
    }

    private async Task<List<Currency>> LoadFromBothAsync()
    {
        var jsonData = await _json.LoadAsync();
        var sqliteData = await _sqlite.LoadAsync();

        // Union by CharCode (case-insensitive). When both contain the same code,
        // prefer the SQLite copy — arbitrary but consistent.
        var merged = new Dictionary<string, Currency>(StringComparer.OrdinalIgnoreCase);

        foreach (var c in jsonData)
        {
            if (c.CharCode != null)
                merged[c.CharCode] = c;
        }

        foreach (var c in sqliteData)
        {
            if (c.CharCode != null)
                merged[c.CharCode] = c;  // Overwrites JSON entry if same code
        }

        return merged.Values.ToList();
    }

    public async Task SaveAsync(List<Currency> currencies)
    {
        // In Both mode, write to both backends so they stay synchronized.
        // Single-mode writes go only to the chosen backend.
        switch (_mode)
        {
            case StorageMode.Json:
                await _json.SaveAsync(currencies);
                break;

            case StorageMode.Sqlite:
                await _sqlite.SaveAsync(currencies);
                break;

            case StorageMode.Both:
                await _json.SaveAsync(currencies);
                await _sqlite.SaveAsync(currencies);
                break;
        }
    }
}