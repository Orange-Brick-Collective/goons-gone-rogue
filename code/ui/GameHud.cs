using Sandbox;
using Sandbox.UI;

namespace GGame;

public class GameHud : Panel {
    public Label score, depth, money;

    public GameHud() {
        StyleSheet.Load("ui/GameHud.scss");
        
        score = new() {Classes = "score"};
        AddChild(score);
        depth = new() {Classes = "depth"};
        AddChild(depth);
        money = new() {Classes = "money"};
        AddChild(money);

        AddChild(new TeamUI());
    }

    public override void Tick() {
        if (Player.Current.InMenu) {
            Style.Opacity = 0;
            return;
        } else {
            Style.Opacity = 1;
        }

        score.SetText($"Score: {GGame.Current.Score}");
        depth.SetText($"Depth: {GGame.Current.CurrentDepth}");
        money.SetText($"Money: {GGame.Current.Money}");
    }
}