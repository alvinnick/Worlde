using System.Net.Http;
using System.IO;

namespace Wordle;

public partial class MainPage : ContentPage
{
	private string localPath = Path.Combine(FileSystem.AppDataDirectory, "words.txt");// Path to store the downloaded word list locally
	private string[] words;// stores the list of words
	private string targetWord;// The target word for the current game


	public MainPage()
	{
		InitializeComponent();
		InitializeWordList(); // Start the process of downloading and initializing the word list
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


	
}

