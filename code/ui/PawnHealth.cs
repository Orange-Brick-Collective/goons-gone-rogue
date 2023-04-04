using Sandbox;
using Sandbox.UI;

namespace GGame;

public class PawnHealth {
    public static PawnHealth Cur {get; set;}

    public PawnHealth() {
        if (Cur is not null) return;
        Cur = this;
        Event.Register(this);
    }

    [Event.Client.Frame]
    public void Frame() {
        var goo = GGame.Current.goons;
        
        for (int i = 0; i < goo.Count; i++) {
            if (goo[i] is null || !goo[i].IsValid()) continue;
            
			if (goo[i].healthPanel is null) {
				WorldPanel e = new();
                e.Style.JustifyContent = Justify.Center;
                e.PanelBounds = new Rect(-4000, -600, 8000, 600);
                e.WorldScale = 0.8f;
				e.AddChild(new GoonStats(goo[i]));
				goo[i].healthPanel = e;
			} else {
                goo[i].healthPanel.Rotation = Rotation.FromYaw(Camera.Rotation.Yaw() + 180);
                goo[i].healthPanel.Position = goo[i].Position + goo[i].HeightOffset * 2.2f;
            }
		}

        if (Player.Current.healthPanel is null) {
            WorldPanel e = new();
            e.Style.JustifyContent = Justify.Center;
            e.PanelBounds = new Rect(-4000, -600, 8000, 600);
            e.WorldScale = 0.8f;

            GoonStats stats = new(Player.Current);
            stats.stats.Style.BackgroundColor = new Color(0, 0, 0, 0.8f);
            e.AddChild(stats);
            
            Player.Current.healthPanel = e;
        } else {
            if (Player.Current.InMenu) {
                Player.Current.healthPanel.Style.Opacity = 0;
                return;
            } else {
                Player.Current.healthPanel.Style.Opacity = 1;
            }

            Player.Current.healthPanel.Rotation = Rotation.FromYaw(Player.Current.Rotation.Yaw() + 180);
            Player.Current.healthPanel.Position = Player.Current.Position + Vector3.Up * 26 + Player.Current.Rotation.Backward * 3.8f;
        }
    }
}