using Sandbox;
using Sandbox.UI;

namespace GGame;

public partial class Hud : HudEntity<RootPanel> {
    public static Hud Current;
    public Panel loadingPanel;
    public FloatingText floatingText;
    public UsePopupPanel usePopupPanel;
    public Crosshair crosshair;

    public Hud() {
        if (Current is not null) return;
        Current = this;
        
        RootPanel.StyleSheet.Load("ui/Hud.scss");
        
        loadingPanel = new Panel(RootPanel, "loadingPanel");

        crosshair = new Crosshair();
        RootPanel.AddChild(crosshair);
        
        floatingText = new FloatingText();
        RootPanel.AddChild(floatingText);

        RootPanel.AddChild(new GameHud());
        RootPanel.AddChild(new Menu());
        
        usePopupPanel = new UsePopupPanel();
        RootPanel.AddChild(usePopupPanel);
    }
    
    public void ToBlack() {
        loadingPanel.AddClass("loading");
    }

    public void FromBlack() {
        loadingPanel.RemoveClass("loading");
    }
}