using Wordle;
using System.Text.Json;
using System.Collections.ObjectModel;

namespace Wordle;

public partial class MainPage : ContentPage
{
	
	private string localPath = Path.Combine(FileSystem.AppDataDirectory, "words.txt");// Path to store the downloaded word list locally
	private string[] words;// stores the list of words
	private string targetWord;// The target word for the current game
    private int gridSize = 5; // Default grid size for Wordle (5 letters)
    private Entry[,] textBoxes; // Array to store all Entry elements
    private int currentRow = 0; // Tracks the current row for guessing
	private bool isChoiceDialogOpen = false;
	private int gamesPlayed = 0; // Total games played
	private int gamesWon = 0;    // Total games won
	private int currentWinStreak = 0; // Current consecutive wins
	private int maxWinStreak = 0;     // Maximum consecutive win streak
	public PlayerStats Stats { get; internal set; }
	public ObservableCollection<PlayerStats> Rankings { get; internal set; }

	public MainPage()
	{
		this.SizeChanged += OnSizeChanged;
		InitializeComponent();
		InitializeWordList(); // Start the process of downloading and initializing the word list
        CreateGameGrid(); // Set up the game grid

		// Initialize player stats
		Stats = new PlayerStats();
		LoadStats();
		Rankings = new ObservableCollection<PlayerStats>(); // Initialize Rankings
		ConfigureKeyboard(); // Adjust keyboard layout for the platform
	

	}
	private void OnSizeChanged(object sender, EventArgs e)
	{
		// Get the available width and height
		double availableWidth = this.Width;
		double availableHeight = this.Height;

		// Calculate sizes dynamically
		double gridSize = Math.Min(availableWidth * 0.9, availableHeight * 0.6); // 90% width or 60% height
		GameGrid.WidthRequest = gridSize;
		GameGrid.HeightRequest = gridSize;

		double keyboardHeight = availableHeight * 0.25; // Keyboard occupies 25% of height
		KeyboardStack.HeightRequest = keyboardHeight;

		// Adjust padding and margins to ensure fit
		MainLayout.Padding = new Thickness(10, 10);
		GameGrid.Margin = new Thickness(0, 10, 0, 10);

		// Center the grid and keyboard
		GameGrid.HorizontalOptions = LayoutOptions.Center;
		GameGrid.VerticalOptions = LayoutOptions.Center;
		KeyboardStack.HorizontalOptions = LayoutOptions.Center;
		KeyboardStack.VerticalOptions = LayoutOptions.End;
	}

	// Initializes the word list by downloading it (if necessary)
	private async void InitializeWordList()
    {
        await DownloadWordListAsync(); // Ensure the word list is downloaded
        LoadWordList(); // Load the words into an array
        SelectTargetWord(); // Randomly select a target word for the game
    }

	// Downloads the word list from a remote URL if not in local file
	async Task DownloadWordListAsync()
	{
		string url = "https://raw.githubusercontent.com/DonH-ITS/jsonfiles/main/words.txt";
		
		// Check if the word list already exists locally
		if (!File.Exists(localPath))
		{
			using HttpClient client = new HttpClient();
			string content = await client.GetStringAsync(url);
			File.WriteAllText(localPath, content); // Save the word list to the local file system
		}
	}
	// Loads the word list from the local file into an array
    void LoadWordList()
    {
        words = File.ReadAllLines(localPath); // Read all lines from the word list file
    }

    // Selects a random word from the loaded word list to be the target word for the game
    void SelectTargetWord()
    {
        Random random = new Random(); // Random number generator
        targetWord = words[random.Next(words.Length)]; // Choose a random word
    }
	private void MoveToNextBox(int row, int col)
    {
        int nextCol = col + 1;

        // Ensure we're not moving out of bounds
        if (nextCol < gridSize)
        {
            textBoxes[row, nextCol]?.Focus(); // Set focus to the next column
        }
    }
	private void CreateGameGrid()
    {
        textBoxes = new Entry[gridSize, gridSize]; // Initialize the array

		// Define grid rows and columns based on gridSize
		for (int i = 0; i < gridSize; i++)
		{
			GameGrid.RowDefinitions.Add(new RowDefinition());
			GameGrid.ColumnDefinitions.Add(new ColumnDefinition());
		}

		// Populate the grid with text boxes
		for (int row = 0; row < gridSize; row++)
		{
			for (int col = 0; col < gridSize; col++)
			{
				var textBox = CreateTextBox(row, col); // Use a helper method
				textBoxes[row, col] = textBox; // Store reference
				Grid.SetRow(textBox, row); // Set row position
				Grid.SetColumn(textBox, col); // Set column position
				GameGrid.Children.Add(textBox); // Add to the grid
			}
		}

	}
	private Entry CreateTextBox(int row, int col)
	{
		var textBox = new Entry
		{
			Placeholder = string.Empty,
			HorizontalTextAlignment = TextAlignment.Center,
			VerticalTextAlignment = TextAlignment.Center,
			FontSize = 18,
			MaxLength = 1,
			IsEnabled = (row == currentRow), // Enable only the current row
			
		};
			
			// Set initial background color based on the app theme
    		UpdateTextBoxTheme(textBox);

		// Handle TextChanged to enforce uppercase and move focus
		textBox.TextChanged += (sender, e) =>
		{
			if (!string.IsNullOrEmpty(e.NewTextValue))
			{
				if (textBox.Text != e.NewTextValue.ToUpper())
				{
					textBox.Text = e.NewTextValue.ToUpper(); // Convert to uppercase
					textBox.CursorPosition = textBox.Text.Length; // Keep cursor at the end
				}
				MoveToNextBox(row, col); // Move to the next box
			}
			//MoveToNextRow();
		};

		return textBox;
	}
    private string GetUserInput()
    {
        string userInput = string.Empty;

        for (int col = 0; col < gridSize; col++)
        {
            var letter = textBoxes[currentRow, col]?.Text; // Assuming the first row for the current guess
            userInput += string.IsNullOrEmpty(letter) ? " " : letter.ToLower(); // Handle empty boxes gracefully
        }

        return userInput;
    }
	private void SubmitGuess(object sender, EventArgs e)
	{
		string userInput = GetUserInput(); // Collect the user's input from the grid

		// Validate input length
		if (userInput.Trim().Length != gridSize)
		{
			DisplayAlert("Incomplete Guess", "Please fill all boxes before submitting.", "OK");
			return;
		}

		// Increment games played if it's the last attempt
		if (currentRow == gridSize - 1 && userInput != targetWord.ToLower())
		{
			Stats.GamesPlayed++;
			gamesPlayed++;
			Stats.CurrentWinStreak = 0; // Reset win streak on loss
			currentWinStreak = 0;
			SaveStats();
			DisplayAlert("Game Over", $"You've used all your guesses! The word was: {targetWord}", "OK");
			ResetGrid();
			return;
		}

		// Check if the guess is correct
		if (userInput == targetWord.ToLower())
		{
			Stats.GamesPlayed++;
			gamesPlayed++;
			Stats.GamesWon++;
			gamesWon++;
			Stats.CurrentWinStreak++;
			currentWinStreak++;

			if (Stats.CurrentWinStreak > Stats.MaxWinStreak)
			{
				Stats.MaxWinStreak = Stats.CurrentWinStreak;
			}
			if (currentWinStreak > maxWinStreak)
			{
				maxWinStreak = currentWinStreak;
			}

			SaveStats(); // Save updated stats
			ResetGameWithChoice();
			UpdateRankings();
		}
		else
		{
			// Provide feedback for the current guess
			ProvideFeedback(userInput, currentRow);

			// Move to the next row for a new guess
			MoveToNextRow();
			currentRow++;
		}
	}
    private void ProvideFeedback(string userInput,int row)
    {
        for (int col = 0; col < gridSize; col++)
        {
            var letterBox = textBoxes[row, col];
            char guessedChar = userInput[col];
            char targetChar = targetWord[col];

            if (guessedChar == targetChar)
            {
                letterBox.BackgroundColor = Colors.Green;// Correct position
            }
            else if (targetWord.Contains(guessedChar))
            {
                letterBox.BackgroundColor = Colors.Yellow;// Misplaced letter
            }
            else
            {
                letterBox.BackgroundColor = Colors.Gray;// Incorrect letter
            }
        }
    }
    private void ResetGame()
	{ 
		// Select a new target word
		SelectTargetWord();

		// Reset the current row
		currentRow = 0;

		// Clear existing grid
		GameGrid.Children.Clear();
		GameGrid.RowDefinitions.Clear();
		GameGrid.ColumnDefinitions.Clear();

		// Recreate the grid
		CreateGameGrid();
		
    }
	private async void ResetGameWithChoice()
	{
		if (isChoiceDialogOpen) return; // Prevent multiple dialogs from opening
		isChoiceDialogOpen = true;

		string action = await DisplayActionSheet(
			"Congratulations! You guessed the word!",
			"Cancel",
			null,
			"View Stats",
			"Start Over",
			"Continue"
		);

		isChoiceDialogOpen = false; // Reset flag after user makes a choice

		if (action == "View Stats")
		{
			await Navigation.PushAsync(new StatsPage(Stats.GamesPlayed, Stats.GamesWon, Stats.CurrentWinStreak, Stats.MaxWinStreak));
			ResetGrid();
		}
		else if (action == "Start Over")
		{
			ResetGame(); // Fully restart the game
		}
		else if (action == "Continue")
		{
			SelectTargetWord(); // Choose a new target word
			ResetGrid(); // Clear the grid without resetting stats
		}

	}
	private void ResetGrid()
	{
		// Reset the current row to the first one
		currentRow = 0;

		// Clear the existing grid
		GameGrid.Children.Clear();
		GameGrid.RowDefinitions.Clear();
		GameGrid.ColumnDefinitions.Clear();

		// Recreate the grid
		CreateGameGrid();
	}
    private void MoveToNextRow()
    {
        for (int col = 0; col < gridSize; col++)
        {
            // Disable the current row
            textBoxes[currentRow, col].IsEnabled = false;

            // Enable the next row if it exists
            if (currentRow + 1 < gridSize)
            {
                textBoxes[currentRow + 1, col].IsEnabled = true;
            }
        }
    }
    private void OnKeyboardButtonClicked(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            string letter = button.Text;

            // Find the first empty box in the current row
            for (int col = 0; col < gridSize; col++)
            {
                var entry = textBoxes[currentRow, col];
                if (string.IsNullOrEmpty(entry.Text))
                {
                    entry.Text = letter; // Fill the box with the letter
                    MoveToNextBox(currentRow, col); // Move to the next box
                    return; // Exit after updating the first empty box
                }
            }
        }
    }
    private void OnBackspaceClicked(object sender, EventArgs e)
    {
        for (int col = gridSize - 1; col >= 0; col--)
        {
            var entry = textBoxes[currentRow, col];
            if (!string.IsNullOrEmpty(entry.Text))
            {
                entry.Text = string.Empty; // Clear the last filled box
                entry.Focus(); // Keep focus on the cleared box
                return; // Exit after clearing the last non-empty box
            }
        }
    }
	private async void OnShowStatsClicked(object sender, EventArgs e)
	{
		// Navigate to the StatsPage and pass stats
		//await Navigation.PushAsync(new StatsPage(Stats.GamesPlayed, Stats.GamesWon, Stats.CurrentWinStreak, Stats.MaxWinStreak));
		// Navigate to the StatsPage and pass stats
		await Navigation.PushAsync(new StatsPage(gamesPlayed, gamesWon, currentWinStreak, maxWinStreak));
	}
	private async void OnSettingsClicked(object sender, EventArgs e)
	{
		// Navigate to the SettingsPage
		await Navigation.PushAsync(new SettingsPage(this));
	}
	private void UpdateTextBoxTheme(Entry textBox)
	{
		if (Application.Current.UserAppTheme == AppTheme.Dark)
		{
			textBox.BackgroundColor = Colors.DarkGray; // Dark mode color
			textBox.TextColor = Colors.White;         // Text color for dark mode
		}
		else
		{
			textBox.BackgroundColor = Colors.LightGray; // Light mode color
			textBox.TextColor = Colors.Black;           // Text color for light mode
		}
	}
	public void UpdateGridTheme()
	{
		for (int row = 0; row < gridSize; row++)
		{
			for (int col = 0; col < gridSize; col++)
			{
				var textBox = textBoxes[row, col];
				UpdateTextBoxTheme(textBox); // Update each box's background color
			}
		}
	}
	public bool ProvideHint()
	{
		if (currentRow >= gridSize)
		{
			return false; // No hints available if the current row is invalid
		}

		for (int col = 0; col < gridSize; col++)
		{
			var textBox = textBoxes[currentRow, col];

			// If the box is empty, fill it with the correct letter
			if (string.IsNullOrEmpty(textBox.Text))
			{
				textBox.Text = targetWord[col].ToString().ToUpper(); // Show the correct letter
				textBox.BackgroundColor = Colors.LightBlue;          // Highlight the hint
				return true;
			}
		}

		return false; // No empty boxes available for hints
	}
	public void SaveStats()
	{
		try
		{
			var statsJson = JsonSerializer.Serialize(Stats);
			File.WriteAllText(Path.Combine(FileSystem.AppDataDirectory, "playerStats.json"), statsJson);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error saving stats: {ex.Message}");
		}
	}
	private void LoadStats()
	{
		try
		{
			var statsFile = Path.Combine(FileSystem.AppDataDirectory, "playerStats.json");
			if (File.Exists(statsFile))
			{
				var statsJson = File.ReadAllText(statsFile);
				var loadedStats = JsonSerializer.Deserialize<PlayerStats>(statsJson);
				if (loadedStats != null)
				{
					Stats = loadedStats;
				}
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error loading stats: {ex.Message}");
		}
	}
	private async void OnSetNameClicked(object sender, EventArgs e)
	{
		// Prompt the user to enter their name
		string name = await DisplayPromptAsync("Set Name", "Enter your name:");
		if (!string.IsNullOrWhiteSpace(name))
		{
			Stats.PlayerName = name; // Update the player name in stats
			SaveStats(); // Save updated stats to local storage

			// Optionally display a confirmation
			await DisplayAlert("Name Set", $"Welcome, {name}!", "OK");
		}
	}
	private async void OnPlayerStatsClicked(object sender, EventArgs e)
	{
		// Navigate to the PlayerStatsPage and pass the current player's stats and rankings
		await Navigation.PushAsync(new PlayerStatsPage(Stats, Rankings));
	}	
	private void UpdateRankings()
	{
		var existingPlayer = Rankings.FirstOrDefault(r => r.PlayerName == Stats.PlayerName);
		if (existingPlayer != null)
		{
			existingPlayer.GamesPlayed = Stats.GamesPlayed;
			existingPlayer.GamesWon = Stats.GamesWon;
			existingPlayer.CurrentWinStreak = Stats.CurrentWinStreak;
			existingPlayer.MaxWinStreak = Stats.MaxWinStreak;
		}
		else
		{
			Rankings.Add(new PlayerStats
			{
				PlayerName = Stats.PlayerName,
				GamesPlayed = Stats.GamesPlayed,
				GamesWon = Stats.GamesWon,
				CurrentWinStreak = Stats.CurrentWinStreak,
				MaxWinStreak = Stats.MaxWinStreak
			});
		}

		SaveStats();
	}

	private void ConfigureKeyboard()
	{
		if (DeviceInfo.Current.Platform == DevicePlatform.Android)
		{
			// Show only "Submit" and "Delete" buttons on Android
			KeyboardStack.Children.Clear();

			var row = new StackLayout
			{
				Orientation = StackOrientation.Horizontal,
				HorizontalOptions = LayoutOptions.Center,
				Spacing = 2
			};

			// Delete Button
			var deleteButton = new Button
			{
				Text = "⌫",
				WidthRequest = 80,
				HeightRequest = 40,
				BackgroundColor = Colors.Red,
				TextColor = Colors.White
			};
			deleteButton.Clicked += OnBackspaceClicked;
			row.Children.Add(deleteButton);

			// Submit Button
			var submitButton = new Button
			{
				Text = "Submit",
				WidthRequest = 80,
				HeightRequest = 40,
				BackgroundColor = Colors.Green,
				TextColor = Colors.White
			};
			submitButton.Clicked += SubmitGuess;
			row.Children.Add(submitButton);

			KeyboardStack.Children.Add(row);
		}
	}

}

