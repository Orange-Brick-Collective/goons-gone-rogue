using Sandbox;
using System.Collections.Generic;

namespace GGame;

public enum TileType {
    Endcap,
    Straight,
    Corner,
    T,
    X
}

public class Level {
    public int length = 12, width = 6;
    public int depth = 0;
    public int wallType = 0;
    public List<List<Tile>> tiles;
}

public class Tile : ModelEntity {
    public int rot = 0;
    public bool[] directions = {false, false, false, false}; // NESW
    public Transform northPoint, eastPoint, southPoint, westPoint; // NESW
    public ModelEntity northWall, eastWall, southWall, westWall; // NESW
    public AnimatedEntity tileEvent;

    public virtual string StreetModel {get; set;}
    public virtual string WallModel {get; set;}

    public Tile() {}
    public Tile(Vector2 position, string wallModel, int rot) {
        Tags.Add("tile");
        if (this is TileEmpty) return;

        Position = position;
        this.rot = rot;

        SetModel(StreetModel);
        SetupPhysicsFromModel(PhysicsMotionType.Static);
        Rotation = Rotation.FromYaw(rot * 90);

        // if (directions[0 + rot % 4]) {
        //     northPoint = Model.GetAttachment("northpoint").Value;
        //     northWall = new();
        //     northWall.SetModel(wallModel);
        //     northWall.Transform = northPoint;
        // }
        // if (directions[1 + rot % 4]) {
        //     eastPoint = Model.GetAttachment("eastpoint").Value;
        //     eastWall = new();
        //     eastWall.SetModel(wallModel);
        //     eastWall.Transform = eastPoint; 
        // }
        // if (directions[2 + rot % 4]) {
        //     southPoint = Model.GetAttachment("southpoint").Value;
        //     southWall = new();
        //     southWall.SetModel(wallModel);
        //     southWall.Transform = southPoint;
        // }
        // if (directions[3 + rot % 4]) {
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
    public override string StreetModel {get; set;} = "models/map/map-straight.vmdl";

    public TileStraight() {}
    public TileStraight(Vector2 position, string wallModel, int rot) : base(position, wallModel, rot) {}
}

public class TileCorner : Tile {
    public new bool[] directions = {true, true, false, false};
    public override string StreetModel {get; set;} = "models/map/map-corner.vmdl";

    public TileCorner() {}
    public TileCorner(Vector2 position, string wallModel, int rot) : base(position, wallModel, rot) {}
}

public class TileT : Tile {
    public new bool[] directions = {true, false, true, true};
    public override string StreetModel {get; set;} = "models/map/map-t-junction.vmdl";

    public TileT() {}
    public TileT(Vector2 position, string wallModel, int rot) : base(position, wallModel, rot) {}
}

public class TileX : Tile {
    public new bool[] directions = {true, true, true, true};
    public override string StreetModel {get; set;} = "models/map/map-x-junction.vmdl";

    public TileX() {}
    public TileX(Vector2 position, string wallModel, int rot) : base(position, wallModel, rot) {}
}

public class TileEmpty : Tile {
    public TileEmpty() : base(Vector2.Zero, "", 0) {}
}