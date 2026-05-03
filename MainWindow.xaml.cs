using System.Windows;
using CurrencyApp.Services;

namespace CurrencyApp;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // Initialize the navigation service with our Frame
        ServiceLocator.Navigation = new NavigationService(RootFrame);

        // Start on the main page
        ServiceLocator.Navigation.NavigateTo("Main");
    }

    private void NavCurrencies_Click(object sender, RoutedEventArgs e) =>
        ServiceLocator.Navigation?.NavigateTo("Main");

    private void NavAdd_Click(object sender, RoutedEventArgs e) =>
        ServiceLocator.Navigation?.NavigateTo("Add");

    private void NavSettings_Click(object sender, RoutedEventArgs e) =>
        ServiceLocator.Navigation?.NavigateTo("Settings");
}