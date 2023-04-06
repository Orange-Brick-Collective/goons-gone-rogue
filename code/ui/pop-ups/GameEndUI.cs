using System;
using Sandbox;
using Sandbox.UI;

namespace GGame;

public class GameEndUI : Panel {
    public Panel left, right, buttons;

    public GameEndUI() {
        StyleSheet.Load("ui/pop-ups/GameEndUI.scss");

        Panel ui = new(this) {Classes = "content"};

        ui.AddChild(new Label() {Classes = "title", Text = "Game Over"});
        ui.AddChild(new Label() {Classes = "description", Text = "-- Goons Gone Rogue --\n\n" +
        $"Depth: {GGame.Current.CurrentDepth}\n" +
        $"Score: {GGame.Current.Score}\n" +
        $"Kills: {GGame.Current.Kills}\n" +
        $"Powerups Used: {GGame.Current.Powerups}\n" + 
        $"Damage Dealt: {GGame.Current.DamageDealt}\n" +
        $"Damage Taken: {GGame.Current.DamageTaken}"
        });

        ui.AddChild(new Button("Return to menu", "", BackToMenu));
    }

    private void BackToMenu() {
        GGame.Current.exploreSong.SetVolume(0);
        GGame.Current.currentSong = GGame.Current.PlaySound("music/menu.sound");
        
        ServerBackToMenu("1209825");
        Delete();
    }
    [ConCmd.Server]
    private static void ServerBackToMenu(string password) {
        if (password != "1209825") return;

        Player.Current.InMenu = true;
        GGame.Current.FromBlackUI();
    }
}