using Sandbox;
using Sandbox.UI;

namespace GGame;

public partial class Hud : HudEntity<RootPanel> {
    public static Hud _hud;
    public EPanel epanel;

    public Hud() {
        if (_hud is not null) return;
        _hud = this;
        
        RootPanel.StyleSheet.Load("ui/Hud.scss");

        RootPanel.AddChild(new Crosshair());
        RootPanel.AddChild(new GameHud());
        RootPanel.AddChild(new Menu());
        
        epanel = new EPanel();
        RootPanel.AddChild(epanel);
    }

    public async void Loading() {
        RootPanel.AddClass("loading");
        await GameTask.DelayRealtime(1200);
        RootPanel.RemoveClass("loading");
    }

}