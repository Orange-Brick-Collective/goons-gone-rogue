using Sandbox;
using Sandbox.UI;

namespace GGame;

public class PowerupUI : Panel {
    public PowerupEntity ent;
    public Pawn player;

    public Pawn chosen;
    public Button chosenButton;

    public PowerupUI(PowerupEntity ent, Pawn player) {
        StyleSheet.Load("ui/pop-ups/PowerupUI.scss");
        this.ent = ent;

        Panel left = new(this) {Classes = "left"};
        left.AddChild(new Button("Close", "", () => {Delete();}) {Classes = "button"});
        left.AddChild(new Image() {Classes = ""});
        left.AddChild(new Label() {Classes = "title", Text = ent.powerup.Title});
        left.AddChild(new Label() {Classes = "description", Text = ent.powerup.Description});

        ////////
        ////////

        Panel right = new(this) {Classes = "right"};

        Panel buttons = new(right) {Classes = "buttonlist"};

        Button plrButton = new("Me", "") {Classes = "button selected"};
        plrButton.AddEventListener("onclick", () => {Select(plrButton, player);});
        buttons.AddChild(plrButton);
        chosen = player;
        chosenButton = plrButton;

        foreach (Pawn pawn in GGame.Cur.goons) {
            if (pawn.Team != player.Team) return;

            Button b = new(pawn.Name, "") {Classes = "button"};
            b.AddEventListener("onclick", () => {Select(b, pawn);});
            buttons.AddChild(b);
        }

        //add button for each teammate goon

        right.AddChild(new Button("Confirm", "", Confirm) {Classes = "button confirmbutton"});
    }

    private void Select(Button chosenButton, Pawn chosen) {
        this.chosenButton.RemoveClass("selected");

        this.chosenButton = chosenButton;
        this.chosen = chosen;
        this.chosenButton.AddClass("selected");
    }

    private void Confirm() {
        ServerConfirm("v8jsod", ent.NetworkIdent, chosen.NetworkIdent);
        Delete();
    }
    
    [ConCmd.Server]
    private static void ServerConfirm(string password, int netIdent, int pawnNetIdent) {
        if (password != "v8jsod") return;
        PowerupEntity ent = Entity.FindByIndex<PowerupEntity>(netIdent);
        ent.powerup.Action.Invoke(Entity.FindByIndex<Pawn>(pawnNetIdent));
        ent.Delete();
    }
}