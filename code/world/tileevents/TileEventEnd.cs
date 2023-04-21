using Sandbox;

namespace GGame;

public class TileEventEnd : TileEvent {
    public override string ModelStr {get; set;} = "models/map/endevent.vmdl";

    public TileEventEnd() {}

    public override void Init(Tile tile) {
        base.Init(tile);
        SetupPhysicsFromAABB(PhysicsMotionType.Static, new Vector3(-104, -104, -12), new Vector3(104, 104, 256));
    }
}