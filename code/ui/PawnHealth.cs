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
        for (int i = 0; i < GGame.Cur.goons.Count; i++) {
            if (goo[i] is null) return;

			if (goo[i].health is null) {
				WorldPanel e = new();
                e.Style.JustifyContent = Justify.Center;
                e.PanelBounds = new Rect(-10000, -400, 20000, 400);
                e.WorldScale = 1.5f;
				e.AddChild(new GoonStats(goo[i]));
				goo[i].health = e;
			}
            
            goo[i].health.Rotation = Rotation.FromYaw(Camera.Rotation.Yaw()+180);
            goo[i].health.Position = goo[i].Position + goo[i].HeightOffset * 2 + Vector3.Up * 4;
		}
    }
}