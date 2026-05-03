using CurrencyApp.Models;

namespace CurrencyApp.Services;

public interface IStorageService
{
    Task<List<Currency>> LoadAsync();
    Task SaveAsync(List<Currency> currencies);
}