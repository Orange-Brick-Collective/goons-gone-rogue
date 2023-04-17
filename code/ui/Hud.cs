using Sandbox;
using Sandbox.UI;

namespace GGame;

public partial class Hud : HudEntity<RootPanel> {
    public static Hud _hud;
    public EPanel epanel;
    public Crosshair crosshair;

    public Hud() {
        if (_hud is not null) return;
        _hud = this;
        
        RootPanel.StyleSheet.Load("ui/Hud.scss");

        crosshair = new Crosshair();
        RootPanel.AddChild(crosshair);

        RootPanel.AddChild(new GameHud());
        RootPanel.AddChild(new Menu());
        
        epanel = new EPanel();
        RootPanel.AddChild(epanel);
    }
    
    public void ToBlack() {
        RootPanel.AddClass("loading");
    }

    public void FromBlack() {
        RootPanel.RemoveClass("loading");
    }

    public void CrosshairInRange(bool inRange) {
        crosshair.SetClass("inrange", inRange);
    }
}