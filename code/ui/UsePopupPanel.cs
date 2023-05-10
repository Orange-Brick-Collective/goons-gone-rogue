using Sandbox.UI;

namespace GGame;

public class UsePopupPanel : Panel {
    public UsePopupPanel() {
        StyleSheet.Load("ui/UsePopupPanel.scss");
        AddChild(new Label() {Text = "(e)"});
    }
}