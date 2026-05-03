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

    public MainViewModel()
    {
        _apiService = new CurrencyApiService();
        _storage = new JsonStorageService();

        _ = LoadFromStorageAsync();
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
}