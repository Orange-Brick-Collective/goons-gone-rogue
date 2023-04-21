using Sandbox;

namespace GGame;

public class TileT : Tile {
    public override string StreetModel {get; set;} = "models/map/map-t-junction.vmdl";

    public TileT() {}
    public TileT(Vector2 position, int rot) : base(position, rot) {
        directions = new[] {true, true, true, false};
    }
}