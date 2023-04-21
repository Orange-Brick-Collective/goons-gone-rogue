using Sandbox;

namespace GGame;

public class TileEvent : ModelEntity {
    public virtual string ModelStr {get; set;} = "";
    public Tile parentTile;

    public TileEvent() {}

    public virtual void Init(Tile tile) {
        parentTile = tile;
        parentTile.tileEvent = this;
        Tags.Add("trigger");
        Tags.Add("generated");
        
        SetModel(ModelStr);
        SetupPhysicsFromAABB(PhysicsMotionType.Static, new Vector3(-224, -224, 0), new Vector3(224, 224, 260));

        Parent = parentTile;
        Position = parentTile.Position;
        Rotation = parentTile.Rotation;
    }
}