using Wordle;
using System.Text.Json;
using System.Collections.ObjectModel;
namespace Wordle;

public partial class SettingsPage : ContentPage
{
    private readonly MainPage _mainPage;
    public SettingsPage(MainPage mainPage)
    {
        InitializeComponent();
        _mainPage = mainPage;

        // Initialize the switch state based on the current app theme
        ThemeSwitch.IsToggled = Application.Current.UserAppTheme == AppTheme.Dark;
    }

    private void OnThemeToggled(object sender, ToggledEventArgs e)
    {
        // Apply the selected theme
        var app = (App)Application.Current;
        app.ApplyTheme(e.Value ? AppTheme.Dark : AppTheme.Light); // Dark if toggled on, Light otherwise

        if (Application.Current.MainPage is NavigationPage nav && nav.CurrentPage is MainPage mainPage)
        {
            mainPage.UpdateGridTheme(); // Update grid colors
        }

        
    }

    private async void OnProvideHintClicked(object sender, EventArgs e)
    {
        if (_mainPage != null)
        {
            bool success = _mainPage.ProvideHint();

            if (!success)
            {
                await DisplayAlert("Hint Not Available", "You can only get hints for the current row.", "OK");
            }
        }
    }

    private async void OnBackToGameClicked(object sender, EventArgs e)
    {
        // Navigate back to the main game page
        await Navigation.PopAsync();
    }

    private void ResetData()
    {
        try
        {
            // Paths to the JSON files
            string statsFile = Path.Combine(FileSystem.AppDataDirectory, "playerStats.json");
            string rankingsFile = Path.Combine(FileSystem.AppDataDirectory, "rankings.json");

            // Delete the files if they exist
            if (File.Exists(statsFile))
            {
                File.Delete(statsFile);
            }

            if (File.Exists(rankingsFile))
            {
                File.Delete(rankingsFile);
            }

            // Clear in-memory data
            var mainPage = Application.Current.MainPage as MainPage;
            if (mainPage != null)
            {
                mainPage.Stats = new PlayerStats();
                mainPage.Rankings = new ObservableCollection<PlayerStats>();
                mainPage.SaveStats();
                
            }

            DisplayAlert("Reset Complete", "All data has been reset.", "OK");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error resetting data: {ex.Message}");
            DisplayAlert("Error", "Failed to reset data. Try again.", "OK");
        }
    }

    private void OnResetDataClicked(object sender, EventArgs e)
    {
        bool confirm = DisplayAlert("Confirm Reset", "Are you sure you want to reset all data?", "Yes", "No").Result;
        if (confirm)
        {
            ResetData();
        }
    }

}

