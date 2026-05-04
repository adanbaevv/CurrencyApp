using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CurrencyApp.Services;

namespace CurrencyApp.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly AppSettingsService _settingsService;

    [ObservableProperty]
    private bool _isJsonSelected;

    [ObservableProperty]
    private bool _isSqliteSelected;

    [ObservableProperty]
    private bool _isBothSelected;

    [ObservableProperty]
    private string? _statusMessage;

    public SettingsViewModel()
    {
        _settingsService = new AppSettingsService();

        var settings = _settingsService.Load();
        IsJsonSelected = settings.StorageMode == StorageMode.Json;
        IsSqliteSelected = settings.StorageMode == StorageMode.Sqlite;
        IsBothSelected = settings.StorageMode == StorageMode.Both;
    }

    [RelayCommand]
    private void Save()
    {
        var settings = _settingsService.Load();

        if (IsSqliteSelected)
            settings.StorageMode = StorageMode.Sqlite;
        else if (IsBothSelected)
            settings.StorageMode = StorageMode.Both;
        else
            settings.StorageMode = StorageMode.Json;

        _settingsService.Save(settings);

        StatusMessage = $"Saved. Restart the app or refresh to apply '{settings.StorageMode}' mode.";
    }
}