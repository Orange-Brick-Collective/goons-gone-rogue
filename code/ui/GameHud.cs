using Sandbox;
using Sandbox.UI;

namespace GGame;

public class GameHud : Panel {
    public Label points, depth;

    public GameHud() {
        StyleSheet.Load("ui/GameHud.scss");
        
        points = new() {Classes = "points"};
        AddChild(points);
        depth = new() {Classes = "depth"};
        AddChild(depth);

        AddChild(new TeamUI());
    }

    public override void Tick() {
        if (Player.Cur.InMenu) {
            Style.Opacity = 0;
            return;
        } else {
            Style.Opacity = 1;
        }

        points.SetText($"Points: {GGame.Cur.Score}");
        depth.SetText($"Depth: {GGame.Cur.CurrentDepth}");
    }
}