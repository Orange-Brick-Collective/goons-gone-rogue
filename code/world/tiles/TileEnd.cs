using Sandbox;

namespace GGame;

public class TileEnd : Tile {
    public override string StreetModel {get; set;} = "models/map/map-endcap.vmdl";

    public TileEnd() {}
    public TileEnd(Vector2 position, int rot) : base(position, rot) {
        directions = new[] {true, false, false, false};
    }
}