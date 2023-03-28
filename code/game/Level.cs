using Sandbox;
using System.Collections.Generic;

namespace GGame;

public class Level {
    public int depth = 0;
    public int wallType = 0;
    public List<List<Tile>> tiles;
}

public class Tile : ModelEntity {
    public bool[] directions = {false, false, false, false}; // NESW
    public Transform northPoint, eastPoint, southPoint, westPoint; // NESW
    public ModelEntity northWall, eastWall, southWall, westWall; // NESW
    public AnimatedEntity tileEvent;

    public virtual string StreetModel {get; set;}
    public virtual string WallModel {get; set;}

    public Tile() {}
    public Tile(Vector3 position, string wallModel) {
        Tags.Add("tile");

        Position = position;

        SetModel(StreetModel);

        // if (directions[0]) {
        //     northPoint = Model.GetAttachment("northpoint").Value;
        //     northWall = new();
        //     northWall.SetModel(wallModel);
        //     northWall.Transform = northPoint;
        // }
        // if (directions[1]) {
        //     eastPoint = Model.GetAttachment("eastpoint").Value;
        //     eastWall = new();
        //     eastWall.SetModel(wallModel);
        //     eastWall.Transform = eastPoint; 
        // }
        // if (directions[2]) {
        //     southPoint = Model.GetAttachment("southpoint").Value;
        //     southWall = new();
        //     southWall.SetModel(wallModel);
        //     southWall.Transform = southPoint;
        // }
        // if (directions[3]) {
        //     westPoint = Model.GetAttachment("westpoint").Value;
        //     westWall = new();
        //     westWall.SetModel(wallModel);
        //     westWall.Transform = westPoint;
        // }
    }

	protected override void OnDestroy() {
        northWall?.Delete();
        eastWall?.Delete();
        southWall?.Delete();
        westWall?.Delete();
        tileEvent?.Delete();
		base.OnDestroy();
	}
}

public class TileStraight : Tile {
    public new bool[] directions = {true, false, true, false};
    public override string StreetModel {get; set;} = "models/map-straight.vmdl";

    public TileStraight() {}
    public TileStraight(Vector3 position, string wallModel) : base(position, wallModel) {}
}

public class TileCorner : Tile {
    public new bool[] directions = {true, true, false, false};
    public override string StreetModel {get; set;} = "models/map-corner.vmdl";

    public TileCorner() {}
    public TileCorner(Vector3 position, string wallModel) : base(position, wallModel) {}
}

public class TileT : Tile {
    public new bool[] directions = {true, false, true, true};
    public override string StreetModel {get; set;} = "models/map-t-junction.vmdl";

    public TileT() {}
    public TileT(Vector3 position, string wallModel) : base(position, wallModel) {}
}

public class TileX : Tile {
    public new bool[] directions = {true, true, true, true};
    public override string StreetModel {get; set;} = "models/map-x-junction.vmdl";

    public TileX() {}
    public TileX(Vector3 position, string wallModel) : base(position, wallModel) {}
}