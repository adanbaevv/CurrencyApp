using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CurrencyApp.Models;
using CurrencyApp.Services;

namespace CurrencyApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly CurrencyApiService _apiService;

    [ObservableProperty]
    private ObservableCollection<Currency> _currencies = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _statusMessage;

    public MainViewModel()
    {
        _apiService = new CurrencyApiService();
    }

    [RelayCommand]
    private async Task LoadCurrenciesAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Loading currencies...";

            var fetched = await _apiService.FetchCurrenciesAsync();

            Currencies.Clear();
            foreach (var currency in fetched)
            {
                Currencies.Add(currency);
            }

            StatusMessage = $"Loaded {Currencies.Count} currencies";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}