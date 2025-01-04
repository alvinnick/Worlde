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
    private int currentRow = 0; // Tracks the current row for guessing


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

                // Handle TextChanged to enforce uppercase
                textBox.TextChanged += (sender, e) =>
                {
                    if(!string.IsNullOrEmpty(e.NewTextValue))
                    {
                        // Prevent recursion by ensuring Text is not repeatedly set
                        if (textBox.Text != e.NewTextValue.ToUpper())
                        {
                            textBox.Text = e.NewTextValue.ToUpper(); // Convert to uppercase
                            textBox.CursorPosition = textBox.Text.Length; // Ensure cursor stays at the end
                        }

                        MoveToNextBox(row, col); // Move focus to the next box
                       
                    }
                };
                
                // Store reference in the textBoxes array
                textBoxes[row, col] = textBox;

                // Set row and column positions for the TextBox
                Grid.SetRow(textBox, row);
                Grid.SetColumn(textBox, col);

                // Add the TextBox to the grid
                GameGrid.Children.Add(textBox);

                textBox.IsEnabled = (row == currentRow); // Only enable the current row

            }

        }

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
        string userInput = GetUserInput(); // Get the user's input from the grid
 
        
        if (userInput.Trim().Length != gridSize)
        {
            DisplayAlert("Incomplete Guess", "Please fill all boxes before submitting.", "OK");
            return;
        }

        ValidateGuess(userInput); // Validate and provide feedback

        if (userInput != targetWord.ToLower()) // If the guess is incorrect
        {
            ProvideFeedback(userInput, currentRow); // Pass currentRow explicitly
            MoveToNextRow();
            currentRow++;

            if (currentRow >= gridSize) // Check if all rows are used
            {
                DisplayAlert("Game Over", $"You've used all your guesses! The word was: {targetWord}", "OK");
                ResetGame(); // Restart the game
            }
        }
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
            ProvideFeedback(userInput,currentRow);
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
        SelectTargetWord();

        // Reset the text and background colors
       /* for (int row = 0; row < gridSize; row++)
        {
            for (int col = 0; col < gridSize; col++)
            {
                var letterBox = textBoxes[row, col];
                letterBox.Text = string.Empty;
                letterBox.BackgroundColor = Colors.White;
            }
        }*/
        

        currentRow = 0; // Reset to the first row
       // MoveToNextRow(); // Enable the first row
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



}

