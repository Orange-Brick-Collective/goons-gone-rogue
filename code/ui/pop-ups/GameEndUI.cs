using System;
using Sandbox;
using Sandbox.UI;

namespace GGame;

public class GameEndUI : Panel {
    public Panel left, right, buttons;

    public GameEndUI() {
        StyleSheet.Load("ui/pop-ups/GameEndUI.scss");

        Panel ui = new(this) {Classes = "content"};

        ui.AddChild(new Label() {Classes = "title", Text = "-- Goons Gone Rogue --\nGame Over"});
        ui.AddChild(new Label() {Classes = "description", Text = 
            $"Score: {GGame.Current.Score}\n" +
            $"Depth: {GGame.Current.CurrentDepth}\n" +
            $"Kills: {GGame.Current.Kills}\n" +
            $"Powerups Bought: {GGame.Current.Purchases}\n" +
            $"Powerups Used: {GGame.Current.Powerups}\n" + 
            $"Damage Dealt: {GGame.Current.DamageDealt:#0}\n" +
            $"Damage Taken: {GGame.Current.DamageTaken:#0}"
        });

        ui.AddChild(new Button("Return to menu", "", BackToMenu));
    }

    private void BackToMenu() {
        Hud.Current.FromBlack();
        ServerBackToMenu("1209825");
        Delete();
    }
    [ConCmd.Server]
    private static void ServerBackToMenu(string password) {
        if (password != "1209825") return;

        if (GGame.Current.IsMusicEnabled) {
			MusicBox.Current.LerpToActive("music/menu.sound");
        }

        Player.Current.InMenu = true;
    }
}