using Sandbox;
using Sandbox.UI;
using System;
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
        left.AddChild(new Image() {Classes = "image", Texture = Texture.Load(FileSystem.Mounted, ent.powerup.Image)});
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
        this.chosenButton.SetText("  " + chosen.Name + "\n" + chosen.PawnStringSingle());

        if (ent.powerup is not PowerupStat powerupStat) return;

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
                this.chosenButton.AddChild(p);
            }
        }
    }

    // idk what P means i just need a special type to easily find/remove
    public class PLabel : Label {}

    private void Confirm() {
        ServerPowerupConfirm("1241568", ent.NetworkIdent, chosen.NetworkIdent);
        Delete();
    }
    
    [ConCmd.Server]
    private static void ServerPowerupConfirm(string password, int netIdent, int pawnNetIdent) {
        if (password != "1241568") return;

        Pawn pawn = Entity.FindByIndex<Pawn>(pawnNetIdent);
        PowerupEntity ent = Entity.FindByIndex<PowerupEntity>(netIdent);
        if (pawn is null || ent is null) return;

        if (ent.powerup is PowerupPawnAct powerupAct) powerupAct.Action.Invoke(pawn);
        else {
            PowerupStat powerupStat = (PowerupStat)ent.powerup;

            foreach (SelectedStat stat in powerupStat.AffectedStats) {
                switch (stat.op) {
                    case Op.Add: {
                        object value = TypeLibrary.GetPropertyValue(pawn, stat.stat.ToString());
                        if (value is float flo) {
                            TypeLibrary.SetProperty(pawn, stat.stat.ToString(), flo + stat.amount);
                        } else {
                            TypeLibrary.SetProperty(pawn, stat.stat.ToString(), (int)value + (int)stat.amount);
                        }
                        break;
                    }
                    case Op.Mult: {
                        object value = TypeLibrary.GetPropertyValue(pawn, stat.stat.ToString());
                        if (value is float flo) {
                            TypeLibrary.SetProperty(pawn, stat.stat.ToString(), flo * stat.amount);
                        } else {
                            TypeLibrary.SetProperty(pawn, stat.stat.ToString(), (int)value * (int)stat.amount);
                        }
                        break;
                    }
                    case Op.Set: {
                        TypeLibrary.SetProperty(pawn, stat.stat.ToString(), stat.amount);
                        break;
                    }
                }
            }
        }

        if (ent.powerup.Title == "Heal Up" || ent.powerup.Title == "Big Heal Up") {
            pawn.Health = Math.Min(pawn.Health + 50, pawn.MaxHealth);

        } else {
            List<AppliedPowerup> p = pawn.AppliedPowerups.Where(a => a.Title == ent.powerup.Title).ToList();
            if (p.Any()) {
                p.First().Title += 1;
            } else {
                pawn.AppliedPowerups.Add(new AppliedPowerup(ent.powerup.Image, ent.powerup.Title));
            }

            GGame.Current.Powerups += 1;
            GGame.Current.Score += 20;
        }

        ent.Delete();
    }
}