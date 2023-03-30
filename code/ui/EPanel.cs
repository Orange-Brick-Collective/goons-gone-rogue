using Sandbox.UI;

namespace GGame;

public class EPanel : Panel {
    public EPanel() {
        StyleSheet.Load("ui/EPanel.scss");
        AddChild(new Label() {Text = "(e)"});
    }
}