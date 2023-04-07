using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;
using System.Linq;

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
        plrButton.Click();

        // add button for each teammate goon
        foreach (Pawn pawn in GGame.Current.goons) {
            if (pawn.Team != 0) return;

            Button b = new(pawn.Name, "") {Classes = "button"};
            b.AddEventListener("onclick", () => {Select(b, pawn);});
            buttons.AddChild(b);
        }
    }

    private void Select(Button chosenButton, Pawn chosen) {
        if (this.chosenButton is not null) {
            foreach(Label lb in this.chosenButton.ChildrenOfType<PLabel>()) {
                lb.Delete();
            }

            this.chosenButton.RemoveClass("selected");
            this.chosenButton.SetText(this.chosen.Name);
        }

        this.chosenButton = chosenButton;
        this.chosen = chosen;

        this.chosenButton.AddClass("selected");
        this.chosenButton.SetText("  " + chosen.Name + "\n" + chosen.PawnString());

        if (ent.powerup.AffectedStats is null) return;
        for (int i = 0; i < 8; i++) {
            List<SelectedStat> a = ent.powerup.AffectedStats.Where(s => (int)s.stat == i).ToList();
            if (a.Any()) {
                PLabel p = new() {Text = $"{a.First().amount}"};
                p.Style.Position = PositionMode.Absolute;
                p.Style.FontColor = a.First().good ? new Color(0, 0.7f, 0) : new Color(0.7f, 0, 0);
                p.Style.Top = Length.Pixels(i * 16 + 17);
                p.Style.Left = Length.Pixels(250);
                this.chosenButton.AddChild(p);
            }
        }
    }

    // idk what P means i just need a special type to easily find/remove
    public class PLabel : Label {}

    private void Confirm() {
        ServerConfirm("124", ent.NetworkIdent, chosen.NetworkIdent);
        Delete();
    }
    
    [ConCmd.Server]
    private static void ServerConfirm(string password, int netIdent, int pawnNetIdent) {
        if (password != "124") return;

        Pawn pawn = Entity.FindByIndex<Pawn>(pawnNetIdent);
        PowerupEntity ent = Entity.FindByIndex<PowerupEntity>(netIdent);
        if (pawn is null || ent is null) return;

        ent.powerup.Action.Invoke(pawn);

        if (ent.powerup.Title != "Heal Up" && ent.powerup.Title != "Big Heal Up") {
            if (pawn.AppliedPowerups.ContainsKey(ent.powerup.Title)) {
                pawn.AppliedPowerups[ent.powerup.Title] += 1;
            } else {
                pawn.AppliedPowerups.Add(ent.powerup.Title, 1);
            }

            GGame.Current.Powerups += 1;
            GGame.Current.Score += 20;
        }

        ent.Delete();
    }
}