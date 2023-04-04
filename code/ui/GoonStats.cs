using Sandbox.UI;

namespace GGame;

public class GoonStats : Panel {
    public Pawn parent;

    public Panel bar;
    public Panel stats, powerups;
    public Panel fill;

    public Label health, numbers, powerupLabel;

    public GoonStats(Pawn parent) {
        StyleSheet.Load("ui/GoonStats.scss");
        this.parent = parent;

        powerups = new(this) {Classes = "stats2"};

        powerupLabel = new() {Classes = "number"};
        powerups.AddChild(powerupLabel);

        stats = new(this) {Classes = "stats"};

        numbers = new() {Classes = "number"};
        stats.AddChild(numbers);

        bar = new(this) {Classes = "bar"};
        fill = new(bar) {Classes = "barfill"};
        _ = new Panel(bar) {Classes = "barborder"};
        health = new() {Classes = "barhealth"};
        bar.AddChild(health);
    }
    
    public override void Tick() {
        bar.Style.Width = parent.MaxHealth;
        fill.Style.Right = Length.Percent(100 - (parent.Health / parent.MaxHealth * 100));

        if (parent.MaxHealth < 200) {
            health.SetText($"{(int)parent.Health}");
        } else {
            health.SetText($"{(int)parent.Health} / {(int)parent.MaxHealth}");
        }

        stats.SetClass("combat", parent.IsInCombat);
        
        if (parent.IsInCombat) {
            numbers.SetText(parent.Name + $"\n {parent.AmmoString()}");
            powerupLabel.SetText(""); 

            if (parent == Player.Current) return;
            parent.healthPanel.WorldScale = 2.5f;
        } else {
            numbers.SetText(parent.PawnString());
            SetPowerupText();

            parent.healthPanel.WorldScale = 0.8f;
        }

        
    }

    public void SetPowerupText() {
        string label = "";
        
        foreach (var thing in parent.AppliedPowerups) {
            label += $"{thing.Key}: {thing.Value}\n";
        }

        powerupLabel.SetText(label); 
    }
}