public class PlayerStats
{
    public string PlayerName { get; set; }    // Player's name
    public int GamesPlayed { get; set; }     // Total games played
    public int GamesWon { get; set; }        // Total games won
    public int CurrentWinStreak { get; set; } // Current consecutive win streak
    public int MaxWinStreak { get; set; }     // Maximum consecutive win streak

    // Calculated win rate based on games played and won
    public double WinRate => (GamesPlayed > 0) ? (double)GamesWon / GamesPlayed * 100 : 0;
}
