using Sandbox;

namespace GGame;

public enum TileType {
    Endcap,
    Straight,
    Corner,
    T,
    X
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
                Transform? p = Model.GetAttachment(attachmentNames[i]);
                if (p is null) return;

                ModelEntity wall = new(walls[System.Random.Shared.Int(walls.Length - 1)], this) {
                    Position = p.Value.Position + Position,
                    Rotation = p.Value.Rotation,
                };
                wall.Tags.Add("generated");
            }
        }
    }

    public void MakeLamp() {
        if (this is TileEmpty || this is TileX) return;

        Transform? p = Model.GetAttachment("lamp");
        if (p is null) return;

        ModelEntity lamp = new("models/map/lamppost.vmdl", this) {
            Position = p.Value.Position * Rotation.FromYaw(rot * 90) + Position,
        };
        lamp.Tags.Add("generated");

        PointLightEntity light = new() {
            Color = new Color(1f, 0.8f, 0.6f),
            Parent = lamp,
            Position = p.Value.Position * Rotation.FromYaw(rot * 90) + Position + Vector3.Up * 204,
        };
        light.Tags.Add("generated");
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