using System.Collections.Generic;
using Sandbox;
using Sandbox.UI;

namespace GGame;

public partial class GoonStats : Panel {
    public Pawn pawn;

    public Panel healthBar, healthBarFill;
    public Label healthNum;
    
    public Panel stat, statNumbers;
    public Label statNameLabel, statIconsLabel, statNumberLabel, statOtherNumberLabel;

    public Panel powerups;

    public GoonStats(Pawn pawn) {
        StyleSheet.Load("ui/GoonStats.scss");
        this.pawn = pawn;
        pawn.stats = this;

        powerups = new(this) {Classes = "powerups"};

        stat = new(this) {Classes = "stats"};

        statNameLabel = new() {Classes = "statnamelabel"};
        stat.AddChild(statNameLabel);

        statNumbers = new(stat) {Classes = "statnumbers"};

        statIconsLabel = new() {Classes = "staticonlabel"};
        statNumbers.AddChild(statIconsLabel);
        statNumberLabel = new() {Classes = "statnumberlabel"};
        statNumbers.AddChild(statNumberLabel);
        statOtherNumberLabel = new() {Classes = "statothernumberlabel"};
        statNumbers.AddChild(statOtherNumberLabel);

        healthBar = new(this) {Classes = "healthbar"};
        healthBarFill = new(healthBar) {Classes = "healthbarfill"};
        _ = new Panel(healthBar) {Classes = "healthbarborder"};
        healthNum = new() {Classes = "healthbarhealth"};
        healthBar.AddChild(healthNum);
    }
    
    public override void Tick() {
        if (pawn is null || !pawn.IsValid) Delete();

        healthBar.Style.Width = pawn.MaxHealth;
        healthBarFill.Style.Right = Length.Percent(100 - (pawn.Health / pawn.MaxHealth * 100));

        if (pawn.MaxHealth < 200) {
            healthNum.SetText($"{(int)pawn.Health}");
        } else {
            healthNum.SetText($"{(int)pawn.Health} / {(int)pawn.MaxHealth}");
        }

        stat.SetClass("combat", pawn.IsInCombat);

        statNameLabel.SetText(pawn.Name);

        if (pawn.IsInCombat) {
            statIconsLabel.SetText("");
            statNumberLabel.SetText(pawn.AmmoString());
            statOtherNumberLabel.SetText("");
            powerups.Style.Opacity = 0;

            if (pawn == Player.Current) {
                pawn.healthPanel.WorldScale = 1;
            } else {
                pawn.healthPanel.WorldScale = 2.5f;
            }   
        } else {
            string[] s = pawn.PawnStrings();

            statIconsLabel.SetText(s[0]);
            statNumberLabel.SetText(s[1]);
            statOtherNumberLabel.SetText(s[2]);
            powerups.Style.Opacity = 1;

            pawn.healthPanel.WorldScale = 0.8f;
        }
    }

    [ClientRpc]
    public static void UpdatePowerups(int pawnIdent) {
        Pawn pawn = (Pawn)Entity.FindByIndex(pawnIdent);
        if (pawn is null) return;

        pawn.stats.powerups.DeleteChildren();

        foreach (AppliedPowerup pow in pawn.AppliedPowerups) {
            pawn.stats.powerups.AddChild(new PowerupDisplay(pow.Image, pow.Title, pow.Amount));
        }
    }

    public class PowerupDisplay : Panel {
        public string title;

        public PowerupDisplay(string img, string title, int amount) {
            this.title = title;
            AddChild(new Image() {Texture = Texture.Load(FileSystem.Mounted, img), Classes = "image"});
            AddChild(new Label() {Text = amount.ToString(), Classes = "label"});
        }
    }
}