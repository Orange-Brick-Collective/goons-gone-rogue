using Sandbox;

namespace GGame;

public class TileEventFight : TileEvent {
    public override string ModelStr {get; set;} = "models/map/enemyevent.vmdl";

    public TileEventFight() {
        RenderColor = new Color(0.5f, 0.3f, 0.3f);
    }
}