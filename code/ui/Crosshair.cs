using Sandbox.UI;

namespace GGame;

public class Crosshair : Panel {
    private readonly Panel dot;
    private readonly Panel circle;

    public Crosshair() {
        dot = new Panel(this, "dot");
        circle = new Panel(this, "circle");
    }

    public void InRange(bool inRange, float spread) {
        dot.SetClass("inrange", inRange);
        circle.SetClass("inrange", inRange);

        Length? size = Length.Pixels(21 * spread);
        circle.Style.Width = size;
        circle.Style.Height = size;
    }
}