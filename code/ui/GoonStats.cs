using Sandbox.UI;

namespace GGame;

public class GoonStats : Panel {
    public Pawn pawn;

    public Panel bar;
    public Panel stats, powerups;
    public Panel fill;

    public Label health, numbers, name, powerupLabel;

    public GoonStats(Pawn pawn) {
        StyleSheet.Load("ui/GoonStats.scss");
        this.pawn = pawn;

        powerups = new(this) {Classes = "stats2"};

        powerupLabel = new() {Classes = "number"};
        powerups.AddChild(powerupLabel);

        stats = new(this) {Classes = "stats"};

        name = new() {Classes = "name"};
        numbers = new() {Classes = "number"};
        stats.AddChild(name);
        stats.AddChild(numbers);

        bar = new(this) {Classes = "bar"};
        fill = new(bar) {Classes = "barfill"};
        _ = new Panel(bar) {Classes = "barborder"};
        health = new() {Classes = "barhealth"};
        bar.AddChild(health);
    }
    
    public override void Tick() {
        if (pawn is null || !pawn.IsValid) Delete();

        bar.Style.Width = pawn.MaxHealth;
        fill.Style.Right = Length.Percent(100 - (pawn.Health / pawn.MaxHealth * 100));

        if (pawn.MaxHealth < 200) {
            health.SetText($"{(int)pawn.Health}");
        } else {
            health.SetText($"{(int)pawn.Health} / {(int)pawn.MaxHealth}");
        }

        stats.SetClass("combat", pawn.IsInCombat);

        name.SetText(pawn.Name);
        if (pawn.IsInCombat) {
            numbers.SetText(pawn.AmmoString());
            powerupLabel.SetText("");

            if (pawn == Player.Current) return;
            pawn.healthPanel.WorldScale = 2.5f;
        } else {
            numbers.SetText(pawn.PawnString());
            SetPowerupText();

            pawn.healthPanel.WorldScale = 0.8f;
        }

        
    }

    public void SetPowerupText() {
        string label = "";
        
        foreach (var thing in pawn.AppliedPowerups) {
            label += $"{thing.Key}: {thing.Value}\n";
        }

        powerupLabel.SetText(label); 
    }
}