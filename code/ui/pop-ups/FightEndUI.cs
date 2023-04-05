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

        Button nothingButton = new("Nothing", "") {Classes = "button"};
        nothingButton.AddEventListener("onclick", Nothing);
        buttons.AddChild(nothingButton);
    }
    
    private void Nothing() {
        ServerNothing("1249");
        Delete();
    }
    [ConCmd.Server]
    private static void ServerNothing(string password) {
        if (password != "1249") return;
        GGame.Current.TransitionEndFight();
    }

    private void Heal() {
        ServerHeal("1247");
        Delete();
    }
    [ConCmd.Server]
    private static void ServerHeal(string password) {
        if (password != "1247") return;
        
		Player.Current.Health = Math.Min(Player.Current.Health + 50, Player.Current.MaxHealth);

		foreach (Goon goon in GGame.Current.goons) {
			if (goon.Team == 0) goon.Health = Math.Min(goon.Health + 50, goon.MaxHealth);
		}

        GGame.Current.TransitionEndFight();
    }

    private void NewPawn() {
        ServerNewPawn("1245");
        Delete();
    }
    [ConCmd.Server]
    private static void ServerNewPawn(string password) {
        if (password != "1245") return;

        Goon g = new();
        g.Init(0, Player.Current);
        g.Generate(GGame.Current.currentWorld.depth);
        g.Position = Player.Current.Position + g.posInGroup;

        GGame.Current.TransitionEndFight();
    }
}