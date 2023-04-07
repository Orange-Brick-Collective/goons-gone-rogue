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
        foreach(PawnPanel p in pawns) {
            if (p.pawn is null || !p.pawn.IsValid) {
                p.Delete();
                return;
            }

            p.SetFill();
        }
    }

    public new void Add(Pawn pawn) {
        PawnPanel a = new(pawn);
        pawns.Add(a);
        TeamUI.Current.AddChild(a);
    }
    public static void Remove(Pawn pawn) {
        var p = pawns.Where(p => p.pawn == pawn);
        if (p.Any()) {
            foreach(var pp in p) {
                pawns.Remove(pp);
                pp.Delete();
                break;
            }
        }
    }
    public void Clear() {
        DeleteChildren();
        pawns = new();
    }

    public class PawnPanel : Panel {
        public Panel bar, fill, health;
        public Label name, healthLabel;
        public Pawn pawn;

        public PawnPanel(Pawn pawn) {
            this.pawn = pawn;

            bar = new(this) {Classes = "bar"};
            fill = new(bar) {Classes = "barfill"};
            _ = new Panel(bar) {Classes = "barborder"};
            health = new() {Classes = "barhealth"};
            bar.AddChild(health);

            name = new() {Classes = "barname"};
            bar.AddChild(name);
            healthLabel = new() {Classes = "barhealth"};
            bar.AddChild(healthLabel);

            if (pawn == Player.Current) {
                bar.AddClass("player");
                AddClass("player");
            }
        }

        public void SetFill() {
            fill.Style.Right = Length.Pixels(200 - (pawn.Health / pawn.MaxHealth * 200));
            healthLabel.SetText($"{(int)pawn.Health} / {(int)pawn.MaxHealth}");
            name.SetText(pawn.Name);
        }
    }
}