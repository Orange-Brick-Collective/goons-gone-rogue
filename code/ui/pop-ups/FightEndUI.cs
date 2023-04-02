using System;
using Sandbox;
using Sandbox.UI;

namespace GGame;

public class FightEndUI : Panel {
    public Panel left, right, buttons;

    public FightEndUI() {
        StyleSheet.Load("ui/pop-ups/FightEndUI.scss");

        Panel ui = new(this) {Classes = "ui"};

        left = new(ui) {Classes = "left"};
        left.AddChild(new Image() {Classes = ""});
        left.AddChild(new Label() {Classes = "title", Text = "Battle complete!"});
        left.AddChild(new Label() {Classes = "description", Text = "Choose your reward"});

        ////////
        ////////

        right = new(ui) {Classes = "right"};
        buttons = new(right) {Classes = "buttonlist"};

        Button healButton = new("Heal team for 50", "") {Classes = "button"};
        healButton.AddEventListener("onclick", Heal);
        buttons.AddChild(healButton);

        Button newPawnButton = new("Get a new goon", "") {Classes = "button"};
        newPawnButton.AddEventListener("onclick", NewPawn);
        buttons.AddChild(newPawnButton);
    }

    private void Heal() {
        ServerHeal("1247");
        Delete();
    }
    
    [ConCmd.Server]
    private static void ServerHeal(string password) {
        if (password != "1247") return;
		Player.Cur.Health = Math.Min(Player.Cur.Health + 50, Player.Cur.MaxHealth);

		foreach (Goon goon in GGame.Cur.goons) {
			if (goon.Team == 0) goon.Health = Math.Min(goon.Health + 50, goon.MaxHealth);
		}

        GGame.Cur.TransitionEndFight();
    }

    private void NewPawn() {
        ServerNewPawn("1245");
        Delete();
    }
    
    [ConCmd.Server]
    private static void ServerNewPawn(string password) {
        if (password != "1245") return;
        Goon g = new();
        g.Init(0, Player.Cur);
        g.Position = Player.Cur.Position + g.posInGroup;

        GGame.Cur.TransitionEndFight();
    }
}