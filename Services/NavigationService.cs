using System.Windows.Controls;
using CurrencyApp.Views;

namespace CurrencyApp.Services;

public class NavigationService : INavigationService
{
    private readonly Frame _frame;

    public NavigationService(Frame frame)
    {
        _frame = frame;
    }

    public void NavigateTo(string pageKey)
    {
        Page page = pageKey switch
        {
            "Main" => new MainPage(),
            "Add" => new AddCurrencyPage(),
            "Settings" => new SettingsPage(),
            _ => throw new ArgumentException($"Unknown page key: {pageKey}")
        };

        _frame.Navigate(page);
    }
}