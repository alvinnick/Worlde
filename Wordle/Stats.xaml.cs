namespace Wordle;
    public partial class StatsPage : ContentPage
    {
        public StatsPage(int gamesPlayed, int gamesWon,int currentWinStreak, int maxWinStreak)
        {
            InitializeComponent();

            // Calculate the win rate
            double winRate = (gamesPlayed > 0) ? (double)gamesWon / gamesPlayed * 100 : 0;

            // Update the stats labels
            GamesPlayedLabel.Text = $"Games Played: {gamesPlayed}";
            GamesWonLabel.Text = $"Games Won: {gamesWon}";
            WinRateLabel.Text = $"Win Rate: {winRate:F1}%";
            CurrentWinStreakLabel.Text = $"Current Win Streak: {currentWinStreak}";
            MaxWinStreakLabel.Text = $"Max Win Streak: {maxWinStreak}";
        }

        private async void OnBackToGameClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync(); // Navigate back to the game page
        }
    }