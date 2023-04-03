using Sandbox;
using Sandbox.UI;

namespace GGame;

public class PowerupUI : Panel {
    public PowerupEntity ent;
    public Pawn player;

    public Pawn chosen;
    public Button chosenButton;

    public Panel left, right, buttons;

    public PowerupUI(PowerupEntity ent, Pawn player) {
        StyleSheet.Load("ui/pop-ups/PowerupUI.scss");
        this.ent = ent;
        this.player = player;

        Panel ui = new(this) {Classes = "ui"};

        left = new(ui) {Classes = "left"};
        left.AddChild(new Button("Close", "", () => {Delete();}) {Classes = "button"});
        left.AddChild(new Image() {Classes = ""});
        left.AddChild(new Label() {Classes = "title", Text = ent.powerup.Title});
        left.AddChild(new Label() {Classes = "description", Text = ent.powerup.Description});

        ////////
        ////////

        right = new(ui) {Classes = "right"};
        buttons = new(right) {Classes = "buttonlist"};
        right.AddChild(new Button("Confirm", "", Confirm) {Classes = "button confirmbutton"});

        Init();
    }

    private async void Init() {
        await GameTask.Delay(100);

        Button plrButton = new(player.PawnString(), "") {Classes = "button selected"};
        plrButton.AddEventListener("onclick", () => {Select(plrButton, player);});
        buttons.AddChild(plrButton);
        chosen = player;
        chosenButton = plrButton;

        // add button for each teammate goon
        foreach (Pawn pawn in GGame.Cur.goons) {
            if (pawn.Team != player.Team) return;

            Button b = new(pawn.Name, "") {Classes = "button"};
            b.AddEventListener("onclick", () => {Select(b, pawn);});
            buttons.AddChild(b);
        }
    }

    private void Select(Button chosenButton, Pawn chosen) {
        this.chosenButton.RemoveClass("selected");
        this.chosenButton.SetText(this.chosen.Name);

        this.chosenButton = chosenButton;
        this.chosen = chosen;

        this.chosenButton.AddClass("selected");
        this.chosenButton.SetText(chosen.PawnString());
    }

    private void Confirm() {
        ServerConfirm("124", ent.NetworkIdent, chosen.NetworkIdent);
        Delete();
    }
    
    [ConCmd.Server]
    private static void ServerConfirm(string password, int netIdent, int pawnNetIdent) {
        if (password != "124") return;

        GGame.Cur.Score += 10;

        PowerupEntity ent = Entity.FindByIndex<PowerupEntity>(netIdent);
        ent.powerup.Action.Invoke(Entity.FindByIndex<Pawn>(pawnNetIdent));
        ent.Delete();
    }
}