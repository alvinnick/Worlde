using System.Net.Http;
using System.IO;

namespace Wordle;

public partial class MainPage : ContentPage
{
	private string localPath = Path.Combine(FileSystem.AppDataDirectory, "words.txt");// Path to store the downloaded word list locally
	private string[] words;// stores the list of words
	private string targetWord;// The target word for the current game
    private int gridSize = 5; // Default grid size for Wordle (5 letters)
    private Entry[,] textBoxes; // Array to store all Entry elements


	public MainPage()
	{
		InitializeComponent();
		InitializeWordList(); // Start the process of downloading and initializing the word list
        CreateGameGrid(); // Set up the game grid
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

	 private void CreateGameGrid()
    {
        textBoxes = new Entry[gridSize, gridSize]; // Initialize the array

        // Define grid rows and columns based on gridSize
        for (int i = 0; i < gridSize; i++)
        {
            GameGrid.RowDefinitions.Add(new RowDefinition());
             GameGrid.ColumnDefinitions.Add(new ColumnDefinition());
        }

        for (int row = 0; row < gridSize ; row++)
        {
            for (int col = 0; col < gridSize; col++)
            {
				var textBox = new Entry // Use Entry for user input
                {
                    Placeholder = string.Empty, // Placeholder text
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                    FontSize = 18,
                    MaxLength = 1 // Limit input to a single character
                };
                
                // Store reference in the textBoxes array
                textBoxes[row, col] = textBox;

                // Set row and column positions for the TextBox
                Grid.SetRow(textBox, row);
                Grid.SetColumn(textBox, col);

                // Add the TextBox to the grid
                GameGrid.Children.Add(textBox);
            }

        }

	}

    private string GetUserInput()
    {
        string userInput = string.Empty;

        for (int col = 0; col < gridSize; col++)
        {
            var letter = textBoxes[0, col]?.Text; // Assuming the first row for the current guess
            userInput += string.IsNullOrEmpty(letter) ? " " : letter.ToLower(); // Handle empty boxes gracefully
        }

        return userInput;
    }
    private void SubmitGuess(object sender, EventArgs e)
    {
        string userInput = GetUserInput(); // Get the user's input from the grid
        ValidateGuess(userInput); // Validate and provide feedback
    }
    private void ValidateGuess(string userInput)
    {
        if (userInput.Trim().Length != gridSize)
        {
            DisplayAlert("Invalid Input", "Please enter a complete word.", "OK");
            return;
        }

        if (userInput == targetWord.ToLower())
        {
            DisplayAlert("Congratulations!", "You guessed the word!", "OK");
            ResetGame();
        }
        else
        {
            ProvideFeedback(userInput);
        }
    }
    private void ProvideFeedback(string userInput)
    {
        for (int col = 0; col < gridSize; col++)
        {
            var letterBox = textBoxes[0, col];
            char guessedChar = userInput[col];
            char targetChar = targetWord[col];

            if (guessedChar == targetChar)
            {
                letterBox.BackgroundColor = Colors.Green;
            }
            else if (targetWord.Contains(guessedChar))
            {
                letterBox.BackgroundColor = Colors.Yellow;
            }
            else
            {
                letterBox.BackgroundColor = Colors.Gray;
            }
        }
    }

    private void ResetGame()
    {
        SelectTargetWord();

        // Reset the text and background colors
        for (int row = 0; row < gridSize; row++)
        {
            for (int col = 0; col < gridSize; col++)
            {
                var letterBox = textBoxes[0, col];
                letterBox.Text = string.Empty;
                letterBox.BackgroundColor = Colors.White;
            }
        }
    }



}

