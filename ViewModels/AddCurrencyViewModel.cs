using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CurrencyApp.Models;
using CurrencyApp.Services;

namespace CurrencyApp.ViewModels;

public partial class AddCurrencyViewModel : ObservableObject
{
    private readonly IStorageService _storage;

    [ObservableProperty]
    private string? _charCode;

    [ObservableProperty]
    private string? _name;

    [ObservableProperty]
    private string? _nominalText = "1";

    [ObservableProperty]
    private string? _valueText;

    [ObservableProperty]
    private string? _statusMessage;

    public AddCurrencyViewModel()
    {
        _storage = new JsonStorageService();
    }

    [RelayCommand]
    private async Task AddAsync()
    {
        // Validate code
        // Normalize code: trim whitespace, uppercase
        var normalizedCode = CharCode?.Trim().ToUpper();

        if (string.IsNullOrWhiteSpace(normalizedCode))
        {
            StatusMessage = "Code is required";
            return;
        }

        // Codes should be 3-letter ISO style (e.g., USD, EUR)
        if (normalizedCode.Length != 3)
        {
            StatusMessage = "Code must be 3 characters";
            return;
        }

        if (!normalizedCode.All(char.IsLetter))
        {
            StatusMessage = "Code must contain only letters";
            return;
        }

        // Validate name
        if (string.IsNullOrWhiteSpace(Name))
        {
            StatusMessage = "Name is required";
            return;
        }

        // Parse and validate nominal
        if (!int.TryParse(NominalText, out var nominal) || nominal <= 0)
        {
            StatusMessage = "Nominal must be a positive whole number";
            return;
        }

        // Parse and validate value — accept both '.' and ',' as decimal separator
        if (!TryParseDecimal(ValueText, out var value) || value <= 0)
        {
            StatusMessage = "Value must be a positive number (e.g., 53.42 or 53,42)";
            return;
        }

        try
        {
            var existing = await _storage.LoadAsync();

            // Prevent duplicate codes
            if (existing.Any(c => c.CharCode?.Equals(normalizedCode, StringComparison.OrdinalIgnoreCase) == true))
            {
                StatusMessage = $"Currency with code '{normalizedCode}' already exists";
                return;
            }

            var newCurrency = new Currency
            {
                CharCode = normalizedCode,
                Name = Name,
                Nominal = nominal,
                Value = value,
                Previous = value,
                IsUserAdded = true,
                Id = $"USER_{Guid.NewGuid():N}"
            };

            existing.Add(newCurrency);
            await _storage.SaveAsync(existing);

            StatusMessage = $"Added {newCurrency.CharCode} successfully";

            // Reset form
            CharCode = string.Empty;
            Name = string.Empty;
            NominalText = "1";
            ValueText = string.Empty;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    private static bool TryParseDecimal(string? input, out decimal result)
    {
        result = 0;
        if (string.IsNullOrWhiteSpace(input))
            return false;

        // Normalize: replace comma with period so InvariantCulture parses both
        var normalized = input.Trim().Replace(',', '.');

        return decimal.TryParse(
            normalized,
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out result);
    }
}