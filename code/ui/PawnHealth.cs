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

    [Event.Tick.Client]
    public void Tick() {
        var goo = GGame.Cur.goons;
        
        for (int i = 0; i < goo.Count; i++) {
            if (goo[i] is null || !goo[i].IsValid()) continue;
            
			if (goo[i].healthPanel is null) {
				WorldPanel e = new();
                e.Style.JustifyContent = Justify.Center;
                e.PanelBounds = new Rect(-4000, -600, 8000, 600);
                e.WorldScale = 1f;
				e.AddChild(new GoonStats(goo[i]));
				goo[i].healthPanel = e;
			} else {
                goo[i].healthPanel.Rotation = Rotation.FromYaw(Camera.Rotation.Yaw() + 180);
                goo[i].healthPanel.Position = goo[i].Position + goo[i].HeightOffset * 2.2f + Vector3.Up * 4;
            }
		}
    }

    [Event.Client.Frame]
    public void Frame() {
        if (Player.Cur.healthPanel is null) {
            WorldPanel e = new();
            e.Style.JustifyContent = Justify.Center;
            e.PanelBounds = new Rect(-4000, -600, 8000, 600);
            e.WorldScale = 0.8f;
            e.AddChild(new GoonStats(Player.Cur));
            Player.Cur.healthPanel = e;
        } else {
            if (Player.Cur.InMenu) {
                Player.Cur.healthPanel.Style.Opacity = 0;
                return;
            } else {
                Player.Cur.healthPanel.Style.Opacity = 1;
            }

            Player.Cur.healthPanel.Rotation = Rotation.FromYaw(Player.Cur.Rotation.Yaw() + 180);
            Player.Cur.healthPanel.Position = Player.Cur.Position + Vector3.Up * 26 + Player.Cur.Rotation.Backward * 3.8f;
        }
    }
}