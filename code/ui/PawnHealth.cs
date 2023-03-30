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
            if (goo[i] is null || !goo[i].IsValid()) return;
            
			if (goo[i].healthPanel is null) {
				WorldPanel e = new();
                e.Style.JustifyContent = Justify.Center;
                e.PanelBounds = new Rect(-10000, -600, 20000, 600);
                e.WorldScale = 1.5f;
				e.AddChild(new GoonStats(goo[i]));
				goo[i].healthPanel = e;
			}
            
            goo[i].healthPanel.Rotation = Rotation.FromYaw(Camera.Rotation.Yaw()+180);
            goo[i].healthPanel.Position = goo[i].Position + goo[i].HeightOffset * 2.2f + Vector3.Up * 4;
		}
    }
}