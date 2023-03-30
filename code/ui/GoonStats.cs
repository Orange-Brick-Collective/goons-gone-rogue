using Sandbox.UI;

namespace GGame;

public class GoonStats : Panel {
    public Pawn parent;

    public Panel bar;
    public Panel numbers;
    public Panel fill;

    public Label health;
    public Label stats;

    public GoonStats(Pawn parent) {
        StyleSheet.Load("ui/GoonStats.scss");
        this.parent = parent;

        numbers = new(this) {Classes = "stats"};

        stats = new() {Classes = "number"};
        numbers.AddChild(stats);

        bar = new(this) {Classes = "bar"};
        fill = new(bar) {Classes = "barfill"};
        _ = new Panel(bar) {Classes = "barborder"};
        health = new() {Classes = "barhealth"};
        bar.AddChild(health);
    }

    public override void Tick() {
        bar.Style.Width = parent.MaxHealth;
        fill.Style.Right = Length.Percent(100 - (parent.Health / parent.MaxHealth * 100));

        numbers.SetClass("combat", !parent.IsInCombat);

        if (!parent.IsInCombat) {
            stats.SetText(parent.Name + $"\n {parent.AmmoString()}");
        } else {
            stats.SetText(parent.PawnString());
        }

        if (parent.MaxHealth < 200) {
            health.SetText($"{(int)parent.Health}");
        } else {
            health.SetText($"{(int)parent.Health} / {(int)parent.MaxHealth}");
        }
    }
}