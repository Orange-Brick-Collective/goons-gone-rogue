using Sandbox;
using Sandbox.UI;

namespace GGame;

public partial class Hud : HudEntity<RootPanel> {
    public static Hud Current;
    public Panel loadingPanel;
    public Panel damagePanel;
    public FloatingText floatingText;
    public UsePopupPanel usePopupPanel;
    public Crosshair crosshair;

    public Hud() {
        if (Current is not null) return;
        Current = this;
        
        RootPanel.StyleSheet.Load("ui/Hud.scss");
        
        loadingPanel = new Panel(RootPanel, "loadingpanel");
        damagePanel = new Panel(RootPanel, "damagepanel");

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

    [ClientRpc]
    public static async void TakeDamage() {
        Hud.Current.damagePanel.AddClass("visible");
        await GameTask.DelayRealtime(100);
        Hud.Current.damagePanel.RemoveClass("visible");
    }
}