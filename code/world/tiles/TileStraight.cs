using Sandbox;

namespace GGame;

public class TileStraight : Tile {
    public override string StreetModel {get; set;} = "models/map/map-straight.vmdl";

    public TileStraight() {}
    public TileStraight(Vector2 position, int rot) : base(position, rot) {
        directions = new[] {true, false, true, false};
    }
}