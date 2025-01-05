namespace Wordle;

public partial class App : Application
{
	public App()
    {
        InitializeComponent();

        // Initialize the theme based on user preference or system default
        InitializeTheme();

        // Set the main page wrapped in a NavigationPage for navigation
        MainPage = new NavigationPage(new MainPage());
    }

    private void InitializeTheme()
    {
        // Load the saved theme preference from Preferences
        string savedTheme = Preferences.Get("AppTheme", "Unspecified");

        // Set the theme based on the saved preference
        UserAppTheme = savedTheme switch
        {
            "Dark" => AppTheme.Dark,
            "Light" => AppTheme.Light,
            _ => AppTheme.Unspecified
        };
    }

    // Method to apply and save the theme dynamically
    public void ApplyTheme(AppTheme theme)
    {
        UserAppTheme = theme; // Apply the theme
        Preferences.Set("AppTheme", theme.ToString()); // Save the user's preference
		// Notify pages to update the UI
		if (MainPage is NavigationPage navigationPage && navigationPage.CurrentPage is MainPage mainPage)
		{
			mainPage.UpdateGridTheme(); // Call a method to update the grid theme
		}
    }
}
