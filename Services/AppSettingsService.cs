using System.IO;
using System.Text.Json;

namespace CurrencyApp.Services;

public class AppSettings
{
    public DateTime? LastSessionTime { get; set; }
    public StorageMode StorageMode { get; set; } = StorageMode.Json;
}

public class AppSettingsService
{
    private readonly string _filePath;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public AppSettingsService()
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "CurrencyApp"
        );

        Directory.CreateDirectory(folder);
        _filePath = Path.Combine(folder, "settings.json");
    }

    public AppSettings Load()
    {
        if (!File.Exists(_filePath))
            return new AppSettings();

        try
        {
            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        catch
        {
            // If the file is corrupted, start fresh rather than crash
            return new AppSettings();
        }
    }

    public void Save(AppSettings settings)
    {
        var json = JsonSerializer.Serialize(settings, JsonOptions);
        File.WriteAllText(_filePath, json);
    }
}