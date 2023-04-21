using Sandbox;

namespace GGame;

public class TileCorner : Tile {
    public override string StreetModel {get; set;} = "models/map/map-corner.vmdl";

    public TileCorner() {}
    public TileCorner(Vector2 position, int rot) : base(position, rot) {
        directions = new[] {true, true, false, false};
    }
}