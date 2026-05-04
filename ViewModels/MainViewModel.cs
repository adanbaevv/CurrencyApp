using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CurrencyApp.Models;
using CurrencyApp.Services;

namespace CurrencyApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly CurrencyApiService _apiService;
    private readonly IStorageService _storage;

    [ObservableProperty]
    private ObservableCollection<Currency> _currencies = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private string? _lastSessionInfo;

    [ObservableProperty]
    private string? _storageModeInfo;

    public MainViewModel()
    {
        _apiService = new CurrencyApiService();

        var settings = new AppSettingsService().Load();
        _storage = new CompositeStorageService(settings.StorageMode);
        StorageModeInfo = $"Storage mode: {settings.StorageMode}";

        LoadSessionInfo();
        _ = LoadFromStorageAsync();
    }

    private void LoadSessionInfo()
    {
        var settingsService = new AppSettingsService();
        var settings = settingsService.Load();

        if (settings.LastSessionTime.HasValue)
        {
            LastSessionInfo = $"Last session: {settings.LastSessionTime.Value:yyyy-MM-dd HH:mm:ss}";
        }
        else
        {
            LastSessionInfo = "First session";
        }

        // Save current session time so the next launch sees it
        settings.LastSessionTime = DateTime.Now;
        settingsService.Save(settings);
    }

    private async Task LoadFromStorageAsync()
    {
        try
        {
            var stored = await _storage.LoadAsync();

            Currencies.Clear();
            foreach (var currency in stored)
            {
                Currencies.Add(currency);
            }

            StatusMessage = stored.Count > 0
                ? $"Loaded {stored.Count} currencies from local storage"
                : "No local data — click 'Refresh' to fetch";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load local data: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Fetching latest rates...";

            var fetched = await _apiService.FetchCurrenciesAsync();

            // Load existing data and preserve user-added entries
            var existing = await _storage.LoadAsync();
            var userAdded = existing.Where(c => c.IsUserAdded).ToList();

            // Merged list: API data first, then user-added entries appended
            var merged = new List<Currency>();
            merged.AddRange(fetched);
            merged.AddRange(userAdded);

            Currencies.Clear();
            foreach (var currency in merged)
            {
                Currencies.Add(currency);
            }

            await _storage.SaveAsync(merged);

            StatusMessage = $"Refreshed: {fetched.Count} from API, {userAdded.Count} user-added";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Refresh failed: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DeleteAsync(Currency? currency)
    {
        if (currency is null)
            return;

        Currencies.Remove(currency);

        // Persist the change
        var current = Currencies.ToList();
        await _storage.SaveAsync(current);

        StatusMessage = $"Deleted {currency.CharCode}";
    }
}