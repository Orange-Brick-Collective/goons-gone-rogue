using Sandbox;

namespace GGame;

public class TileEventStart : TileEvent {
    public override string ModelStr {get; set;} = "models/map/startevent.vmdl";

    public TileEventStart() {}

    public override void Init(Tile tile) {
        base.Init(tile);
        SetupPhysicsFromAABB(PhysicsMotionType.Static, new Vector3(-104, -104, -12), new Vector3(104, 104, 256));
    }
}