using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace GGame;

public partial class Leaderboards : BaseNetworkable {
    public static Leaderboards Current {get; set;}
    [Net] public IDictionary<string, int> TopScores {get; set;} = new Dictionary<string, int>();
    int playerScores = 0;

    public Leaderboards() {
        if (Current is not null) return;
        Current = this;

        TopScores.Add("P-RANK", 22000);
        TopScores.Add("S-RANK", 18000);
        TopScores.Add("A-RANK", 14000);
        TopScores.Add("B-RANK", 10000);
        TopScores.Add("C-RANK", 8000);
        TopScores.Add("D-RANK", 5000);
        TopScores.Add("E-RANK", 2000);
    }

    public void AddScore(int points) {
        Log.Info("Adding Score");
        playerScores += 1;

        TopScores.Add("PLR" + playerScores.ToString().PadLeft(3, '0'), points);
        TopScores = TopScores.OrderByDescending(p => p.Value).ToDictionary(p => p.Key, p => p.Value);

        if (TopScores.Count > 12) {
            TopScores.Remove(TopScores.Last().Key);
        }
    }
}