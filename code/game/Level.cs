using Sandbox;
using System.Collections.Generic;

namespace GGame;

public class Arena {
    public int wallType = 0;
}

public class World {
    public int length = 12, width = 6;
    public int depth = 0;
    public int wallType = 0;
    public List<List<Tile>> tiles;
}

public enum TileType {
    Endcap,
    Straight,
    Corner,
    T,
    X
}

public static class WallModels {
    public static string[] GetModels(int i) {
        return (i % 2) switch {
            1 => level1,
            2 => level2,
            _ => level0,
        };
    }

    public static readonly string[] level0 = {
        "models/map/walls/wall0-full.vmdl",
        "models/map/walls/wall0-half.vmdl",
        "models/map/walls/wall0-half.vmdl",
        "models/map/walls/wall0-flat.vmdl",
        "models/map/walls/wall0-flat.vmdl",
    };

    public static readonly string[] level1 = {
        "models/map/walls/town0-wall0.vmdl",
    };

    public static readonly string[] level2 = {
        "models/map/walls/town0-wall0.vmdl",
    };
}

public class Tile : ModelEntity {
    public int rot = 0;
    public bool[] directions = {false, false, false, false}; // NESW
    public string[] attachmentNames = {"northwall", "eastwall", "southwall", "westwall"};
    public Transform northPoint, eastPoint, southPoint, westPoint; // NESW
    public ModelEntity northWall, eastWall, southWall, westWall; // NESW
    public ModelEntity tileEvent;

    public virtual string StreetModel {get; set;}
    public virtual string WallModel {get; set;}

    public Tile() {}
    public Tile(Vector2 position, int rot) {
        Tags.Add("generated");
        if (this is TileEmpty) return;

        Position = position;
        this.rot = rot;

        SetModel(StreetModel);
        SetupPhysicsFromModel(PhysicsMotionType.Static);
        Rotation = Rotation.FromYaw(rot * 90);
    }

    public void MakeWalls(int wallType) {
        if (this is TileEmpty) return;

        string[] walls = WallModels.GetModels(wallType);

        for (int i = 0; i < 4; i++) {
            if (!directions[(i + rot) % 4]) {
                Transform p = Model.GetAttachment(attachmentNames[i]).Value;
                ModelEntity wall = new(walls[System.Random.Shared.Int(0, walls.Length-1)]) {
                    Position = p.Position + Position,
                    Rotation = p.Rotation,
                    Parent = this,
                };
                wall.Tags.Add("generated");
            }
        }
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

public class TileEnd : Tile {
    public override string StreetModel {get; set;} = "models/map/map-endcap.vmdl";

    public TileEnd() {}
    public TileEnd(Vector2 position, int rot) : base(position, rot) {
        directions = new[] {true, false, false, false};
    }
}

public class TileStraight : Tile {
    public override string StreetModel {get; set;} = "models/map/map-straight.vmdl";

    public TileStraight() {}
    public TileStraight(Vector2 position, int rot) : base(position, rot) {
        directions = new[] {true, false, true, false};
    }
}

public class TileCorner : Tile {
    public override string StreetModel {get; set;} = "models/map/map-corner.vmdl";

    public TileCorner() {}
    public TileCorner(Vector2 position, int rot) : base(position, rot) {
        directions = new[] {true, true, false, false};
    }
}

public class TileT : Tile {
    public override string StreetModel {get; set;} = "models/map/map-t-junction.vmdl";

    public TileT() {}
    public TileT(Vector2 position, int rot) : base(position, rot) {
        directions = new[] {true, true, true, false};
    }
}

public class TileX : Tile {
    public override string StreetModel {get; set;} = "models/map/map-x-junction.vmdl";

    public TileX() {}
    public TileX(Vector2 position, int rot) : base(position, rot) {
        directions = new[] {true, true, true, true};
    }
}

public class TileEmpty : Tile {
    public TileEmpty() : base(Vector2.Zero, 0) {}
}

// *
// * event
// *

public class TileEvent : ModelEntity {
    public virtual string ModelStr {get; set;} = "";
    public Tile parentTile;

    public TileEvent() {}

    public virtual void Init(Tile tile) {
        parentTile = tile;
        Tags.Add("trigger");
        Tags.Add("generated");
        
        SetModel(ModelStr);
        SetupPhysicsFromAABB(PhysicsMotionType.Static, new Vector3(-224, -224, 0), new Vector3(224, 224, 260));

        Parent = parentTile;
        Position = parentTile.Position;
        Rotation = parentTile.Rotation;
    }
}

public class TileEventFight : TileEvent {
    public override string ModelStr {get; set;} = "models/map/enemyevent.vmdl";

    public TileEventFight() {}
}

public class TileEventPowerups : TileEvent {
    public override string ModelStr {get; set;} = "models/map/powerupevent.vmdl";

    public TileEventPowerups() {}

    public override void Init(Tile tile) {
        parentTile = tile;
        Tags.Add("generated");

        SetModel(ModelStr);
        SetupPhysicsFromModel(PhysicsMotionType.Static);

        Parent = parentTile;
        Position = parentTile.Position;
        Rotation = parentTile.Rotation;

        _ = new PowerupEntity() {
            Parent = this,
            Position = Position + new Vector3(-128, 0, 48) * Rotation,
        }.Init(Powerups.GetRandomIndex);

        _ = new PowerupEntity() {
            Parent = this,
            Position = Position + new Vector3(-128, 64, 48) * Rotation,
        }.Init(Powerups.GetRandomIndex);

        _ = new PowerupEntity() {
            Parent = this,
            Position = Position + new Vector3(-128, -64, 48) * Rotation,
        }.Init(Powerups.GetRandomIndex);
    }
}

public class TileEventStart : TileEvent {
    public override string ModelStr {get; set;} = "models/map/startevent.vmdl";

    public TileEventStart() {}

    public override void Init(Tile tile) {
        base.Init(tile);
        SetupPhysicsFromAABB(PhysicsMotionType.Static, new Vector3(-128, -128, 0), new Vector3(128, 128, 260));
    }
}

public class TileEventEnd : TileEvent {
    public override string ModelStr {get; set;} = "models/map/endevent.vmdl";

    public TileEventEnd() {}

    public override void Init(Tile tile) {
        base.Init(tile);
        SetupPhysicsFromAABB(PhysicsMotionType.Static, new Vector3(-128, -128, 0), new Vector3(128, 128, 260));
    }
}

// challenge tile?