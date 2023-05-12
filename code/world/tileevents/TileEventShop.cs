using Sandbox;

namespace GGame;

public class TileEventShop : TileEvent {
    public override string ModelStr {get; set;} = "models/map/shopevent.vmdl";

    public TileEventShop() {}

    public override void Init(Tile tile) {
        parentTile = tile;
        parentTile.tileEvent = this;
        Tags.Add("generated");

        SetModel(ModelStr);
        SetupPhysicsFromModel(PhysicsMotionType.Static);

        Parent = parentTile;
        Position = parentTile.Position;
        Rotation = parentTile.Rotation;

        _ = new ShopEntity() {
            Parent = this,
            Position = Position + new Vector3(-128, 0, 48) * Rotation,
        }.Init();
    }
}