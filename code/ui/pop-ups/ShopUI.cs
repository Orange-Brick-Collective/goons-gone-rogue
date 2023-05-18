using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GGame;

public partial class ShopUI : Panel {
    public ShopEntity ent;
    public Pawn player;

    public Powerup selectedPowerup;
    public Pawn chosen;
    public Button selectedButton, selectedPowerupButton, confirmButton;
    public Label title, description;
    public Panel left, right, buttons, powerupButtons;

    public int cost = 1000;

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
                    // no matter what i did modifying the powerup would lead to the original powerup list changing
                    float mult = stat.op == Op.Add ? 2 : 1.333f;
                    mult = stat.good ? mult : 1;

                    textColor = stat.good ? new Color(0, 0.7f, 0) : new Color(0.7f, 0, 0);
                    p.Text = stat.amount > 0 ? $"+{stat.amount * mult}" : $"{stat.amount * mult}";
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

        if (powerup is PowerupStat) cost = 1000;
        else cost = 200;

        confirmButton.SetText($"Buy {selectedPowerup.Title} for ${cost}");
        Select(selectedButton, chosen);
    }

    // idk what P means i just need a special type to easily find/remove
    public class PLabel : Label {}

    private void Confirm() {
        if (selectedPowerup is null || GGame.Current.Money < cost) {BuyFail(); return;}

        ServerShopConfirm("1582726", ent.NetworkIdent, chosen.NetworkIdent, ent.powerups.IndexOf(selectedPowerup), cost);
        Delete();
    }
    
    [ConCmd.Server]
    private static void ServerShopConfirm(string password, int shopNetIdent, int pawnNetIdent, int selectedPowerup, int cost) {
        if (password != "1582726") return;
        
        if (GGame.Current.Money < cost) return;

        Pawn pawn = Entity.FindByIndex<Pawn>(pawnNetIdent);
        ShopEntity ent = Entity.FindByIndex<ShopEntity>(shopNetIdent);
        if (pawn is null || ent is null) return;

        GGame.Current.Money -= cost;
        
        Powerup powerup = ent.powerups[selectedPowerup];

        if (powerup is PowerupPawnAct powerupAct) powerupAct.Action.Invoke(pawn);
        else {
            PowerupStat powerupStat = (PowerupStat)powerup;

            foreach (SelectedStat stat in powerupStat.AffectedStats) {
                switch (stat.op) {
                    case Op.Add: {
                        float mult = stat.good ? 2 : 1;
                        object value = TypeLibrary.GetPropertyValue(pawn, stat.stat.ToString());
                        if (value is float flo) {
                            TypeLibrary.SetProperty(pawn, stat.stat.ToString(), flo + stat.amount * mult);
                        } else {
                            TypeLibrary.SetProperty(pawn, stat.stat.ToString(), (int)((int)value + stat.amount * mult));
                        }
                        break;
                    }
                    case Op.Mult: {
                        float mult = stat.good ? 1.333f : 1;
                        object value = TypeLibrary.GetPropertyValue(pawn, stat.stat.ToString());
                        if (value is float flo) {
                            TypeLibrary.SetProperty(pawn, stat.stat.ToString(), flo * stat.amount * mult);
                        } else {
                            TypeLibrary.SetProperty(pawn, stat.stat.ToString(), (int)((int)value * stat.amount * mult));
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

        if (powerup.Title == "Heal Up" || powerup.Title == "Big Heal Up") {
            pawn.Health = Math.Min(pawn.Health + 50, pawn.MaxHealth);
        } else {
            List<AppliedPowerup> p = pawn.AppliedPowerups.Where(a => a.Title == powerup.Title).ToList();
            if (p.Any()) {
                p.First().Amount += 1;
            } else {
                pawn.AppliedPowerups.Add(new AppliedPowerup(powerup.Image, powerup.Title));
            }
            GoonStats.UpdatePowerups(pawn.NetworkIdent);

            GGame.Current.Powerups += 1;
            GGame.Current.Purchases += 1;
            GGame.Current.Score += 50;
        }

        if (ent.powerups.Count == 1) ent.Delete();
        else {
            ent.powerups.RemoveAt(selectedPowerup);
            ClientServerShopConfirm(shopNetIdent, selectedPowerup);
        }
    }
    [ClientRpc]
    public static void ClientServerShopConfirm(int shopNetIdent, int selectedPowerup) {
        ShopEntity ent = Entity.FindByIndex<ShopEntity>(shopNetIdent);
        if (ent is null) return;

        ent.powerups.RemoveAt(selectedPowerup);
    }

    private async void BuyFail() {
        confirmButton.Style.BackgroundColor = new Color32(255, 120, 120);
        await GameTask.DelayRealtime(400);
        confirmButton.Style.BackgroundColor = new Color32(195, 202, 207);
    }
}