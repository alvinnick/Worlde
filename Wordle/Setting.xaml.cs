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

}
