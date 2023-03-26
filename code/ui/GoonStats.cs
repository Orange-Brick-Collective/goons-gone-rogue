using Sandbox.UI;

namespace GGame;

public class GoonStats : Panel {
    public Pawn parent;

    public Panel bar;
    public Panel numbers;
    public Panel fill;

    public Label health;

    public Label damage;
    public Label firerate;
    public Label range;

    public GoonStats(Pawn parent) {
        StyleSheet.Load("ui/GoonStats.scss");
        this.parent = parent;

        numbers = new(this) {Classes = "numbers"};

        damage = new() {Classes = "number"};
        numbers.AddChild(damage);
        firerate = new() {Classes = "number"};
        numbers.AddChild(firerate);
        range = new() {Classes = "number"};
        numbers.AddChild(range);

        bar = new(this) {Classes = "bar"};
        fill = new(bar) {Classes = "barfill"};
        _ = new Panel(bar) {Classes = "barborder"};
        health = new() {Classes = "barhealth"};
        bar.AddChild(health);
    }

    public override void Tick() {
        bar.Style.Width = parent.MaxHealth;
        fill.Style.Right = Length.Percent(100 - (parent.Health / parent.MaxHealth * 100));

        damage.SetText($"Damage: {parent.weaponDamage}");
        firerate.SetText($"Firerate: {parent.fireRate}");
        range.SetText($"Range: {0}");

        if (parent.MaxHealth < 200) {
            health.SetText($"{(int)parent.Health}");
        } else {
            health.SetText($"{(int)parent.Health} / {(int)parent.MaxHealth}");
        }
    }
}