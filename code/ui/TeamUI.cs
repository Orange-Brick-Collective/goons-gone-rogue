using Sandbox;
using Sandbox.UI;
using System.Linq;
using System.Collections.Generic;

namespace GGame;

public class TeamUI : Panel {
    public static TeamUI Current {get; set;}
    public static List<PawnPanel> pawns = new();

    public TeamUI() {
        if (Current is not null) return;
        Current = this;
    }

    public override void Tick() {
        foreach(PawnPanel p in pawns) p.SetFill();
    }

    public new static void Add(Goon goon) {
        PawnPanel a = new(goon);
        pawns.Add(a);
        TeamUI.Current.AddChild(a);
    }
    public static void Remove(Goon goon) {
        var p = pawns.Where(p => p.goon == goon);
        if (p.Any()) {
            foreach(var pp in p) {
                pawns.Remove(pp);
                pp.Delete();
                break;
            }
        }
    }

    public class PawnPanel : Panel {
        public Panel bar, fill, health;
        public Label name, healthLabel;
        public Goon goon;

        public PawnPanel(Goon goon) {
            this.goon = goon;

            bar = new(this) {Classes = "bar"};
            fill = new(bar) {Classes = "barfill"};
            _ = new Panel(bar) {Classes = "barborder"};
            health = new() {Classes = "barhealth"};
            bar.AddChild(health);

            name = new() {Classes = "barname"};
            bar.AddChild(name);
            healthLabel = new() {Classes = "barhealth"};
            bar.AddChild(healthLabel);
        }

        public void SetFill() {
            fill.Style.Right = Length.Pixels(200 - (goon.Health / goon.MaxHealth * 200));
            healthLabel.SetText($"{(int)goon.Health} / {(int)goon.MaxHealth}");
            name.SetText(goon.Name);
        }
    }
}