namespace Wordle;
using System.Collections.ObjectModel;
public partial class PlayerStatsPage : ContentPage
{
    public ObservableCollection<PlayerStats> Rankings { get; private set; }
    public PlayerStats CurrentPlayerStats { get; private set; }

    public PlayerStatsPage(PlayerStats playerStats, ObservableCollection<PlayerStats> rankings)
    {
        InitializeComponent();

        CurrentPlayerStats = playerStats;
        Rankings = new ObservableCollection<PlayerStats>(rankings.OrderByDescending(r => r.GamesWon).ThenByDescending(r => r.MaxWinStreak));

        BindingContext = this;
    }
}
