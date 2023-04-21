using Sandbox;

namespace GGame;

public class TileX : Tile {
    public override string StreetModel {get; set;} = "models/map/map-x-junction.vmdl";

    public TileX() {}
    public TileX(Vector2 position, int rot) : base(position, rot) {
        directions = new[] {true, true, true, true};
    }
}