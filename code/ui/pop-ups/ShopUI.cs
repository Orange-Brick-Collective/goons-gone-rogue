using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GGame;

public class ShopUI : Panel {
    public ShopEntity ent;
    public Pawn player;

    public Powerup selectedPowerup;
    public Pawn chosen;
    public Button selectedButton, selectedPowerupButton, confirmButton;
    public Label title, description;
    public Panel left, right, buttons, powerupButtons;

    public ShopUI(ShopEntity ent, Pawn player) {
        StyleSheet.Load("ui/pop-ups/ShopUI.scss");
        this.ent = ent;
        this.player = player;

        Panel ui = new(this) {Classes = "ui"};

        left = new(ui) {Classes = "left"};
        left.AddChild(new Button("Close", "", () => {Delete();}) {Classes = "button"});
        left.AddChild(new Label() {Classes = "text", Text = "What're ya buyin"});
        title = new Label() {Classes = "title", Text = ""};
        left.AddChild(title);
        description = new Label() {Classes = "description", Text = ""};
        left.AddChild(description);

        powerupButtons = new(right) {Classes = "buttonlist"};
        left.AddChild(powerupButtons);

        ////////
        ////////

        right = new(ui) {Classes = "right"};
        buttons = new(right) {Classes = "buttonlist"};

        confirmButton = new Button("Buy ___ for $1000", "", Confirm) {Classes = "button confirmbutton"};
        right.AddChild(confirmButton);

        Init();
    }

    private async void Init() {
        await GameTask.Delay(100);

        Button plrButton = new(player.PawnStringSingle(), "") {Classes = "button selected"};
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

        // add button for each powerup
        foreach (Powerup powerup in ent.powerups) {
            Button b = new(powerup.Title, "") {Classes = "button"};
            b.AddChild(new Image() {Classes = "image", Texture = Texture.Load(FileSystem.Mounted, powerup.Image)});
            b.AddEventListener("onclick", () => {SelectPowerup(b, powerup);});
            powerupButtons.AddChild(b);
        }
    }

    private void Select(Button selectedButton, Pawn chosen) {
        if (this.selectedButton is not null) {
            foreach(Label lb in this.selectedButton.ChildrenOfType<PLabel>()) {
                lb.Delete();
            }
        }

        this.selectedButton?.RemoveClass("selected");
        this.selectedButton?.SetText(this.chosen.Name);

        this.selectedButton = selectedButton;
        this.chosen = chosen;

        this.selectedButton?.AddClass("selected");
        this.selectedButton?.SetText("  " + chosen.Name + "\n" + chosen.PawnStringSingle());

        if (selectedPowerup is not PowerupStat powerupStat) return;

        for (int i = 0; i < Enum.GetNames(typeof(Stat)).Length; i++) {
            List<SelectedStat> a = powerupStat.AffectedStats.Where(s => (int)s.stat == i).ToList();
            
            if (a.Any()) {
                SelectedStat stat = a.First();

                PLabel p = new();
                p.Style.Position = PositionMode.Absolute;

                Color textColor;
                if (stat.op == Op.Set) {
                    textColor = new Color(0.7f, 0, 0);
                    p.Text = $"{stat.amount}";
                } else {
                    textColor = stat.good ? new Color(0, 0.7f, 0) : new Color(0.7f, 0, 0);
                    p.Text = stat.amount > 0 ? $"+{stat.amount}" : $"{stat.amount}";
                }

                p.Style.FontColor = textColor;
                p.Style.Top = Length.Pixels(i * 16 + 17);
                p.Style.Left = Length.Pixels(275);
                this.selectedButton.AddChild(p);
            }
        }
    }

    private void SelectPowerup(Button selectedPowerupButton, Powerup powerup) {
        this.selectedPowerupButton?.RemoveClass("powerupselected");

        this.selectedPowerupButton = selectedPowerupButton;
        this.selectedPowerupButton.AddClass("powerupselected");
        selectedPowerup = powerup;

        description.SetText(selectedPowerup.Description);
        title.SetText(selectedPowerup.Title);

        confirmButton.SetText($"Buy {selectedPowerup.Title} for $1000");
        Select(selectedButton, chosen);
    }

    // idk what P means i just need a special type to easily find/remove
    public class PLabel : Label {}

    private void Confirm() {
        if (selectedPowerup is null) return;
        if (GGame.Current.Money < 1000) return;
        Log.Info("eb");
        ServerConfirm("ee", ent.NetworkIdent, chosen.NetworkIdent, ent.powerups.IndexOf(selectedPowerup));
        Delete();
    }
    
    [ConCmd.Server]
    private static void ServerConfirm(string password, int shopNetIdent, int pawnNetIdent, int selectedPowerup) {
        Log.Info("gottem");
        if (password != "ee") return;
        
        if (GGame.Current.Money < 1000) return;

        Pawn pawn = Entity.FindByIndex<Pawn>(pawnNetIdent);
        ShopEntity ent = Entity.FindByIndex<ShopEntity>(shopNetIdent);
        if (pawn is null || ent is null) return;

        GGame.Current.Money -= 1000;
        

        Powerup powerup = ent.powerups[selectedPowerup];

        if (powerup is PowerupPawnAct powerupAct) powerupAct.Action.Invoke(pawn);
        else {
            PowerupStat powerupStat = (PowerupStat)powerup;

            foreach (SelectedStat selected in powerupStat.AffectedStats) {
                switch (selected.op) {
                    case Op.Add: {
                        object value = TypeLibrary.GetPropertyValue(pawn, selected.stat.ToString());
                        if (value is float flo) {
                            TypeLibrary.SetProperty(pawn, selected.stat.ToString(), flo + selected.amount);
                        } else {
                            TypeLibrary.SetProperty(pawn, selected.stat.ToString(), (int)value + (int)selected.amount);
                        }
                        break;
                    }
                    case Op.Mult: {
                        object value = TypeLibrary.GetPropertyValue(pawn, selected.stat.ToString());
                        if (value is float flo) {
                            TypeLibrary.SetProperty(pawn, selected.stat.ToString(), flo * selected.amount);
                        } else {
                            TypeLibrary.SetProperty(pawn, selected.stat.ToString(), (int)value * (int)selected.amount);
                        }
                        break;
                    }
                    case Op.Set: {
                        TypeLibrary.SetProperty(pawn, selected.stat.ToString(), selected.amount);
                        break;
                    }
                }
            }
        }

        if (powerup.Title == "Heal Up" || powerup.Title == "Big Heal Up") {
            pawn.Health = Math.Min(pawn.Health + 50, pawn.MaxHealth);

        } else {
            if (pawn.AppliedPowerups.ContainsKey(powerup.Title)) {
                pawn.AppliedPowerups[powerup.Title] += 1;
            } else {
                pawn.AppliedPowerups.Add(powerup.Title, 1);
            }

            GGame.Current.Powerups += 1;
            GGame.Current.Score += 20;
        }

        if (ent.powerups.Count == 0) ent.Delete();
        else ent.powerups.RemoveAt(selectedPowerup);
    }
}