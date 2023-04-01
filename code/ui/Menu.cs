using Sandbox.UI;

namespace GGame;

public class Menu : Panel {
    public Menu() {
        StyleSheet.Load("ui/Menu.scss");

        AddChild(new Label() {Text = "Goons Gone Rogue", Classes = "title"});
        Panel buttons = new(this, "buttons");
        buttons.AddChild(new Button("Start","", () => {GGame.TransitionStart("dpiol");}) {Classes = "buttone"});
        buttons.AddChild(new Button("Beginner Info","", () => {}) {Classes = "buttone"});
        buttons.AddChild(new Button("...","", () => {}) {Classes = "buttone"});
        buttons.AddChild(new Button("About","", () => {}) {Classes = "buttone"});
    }
}