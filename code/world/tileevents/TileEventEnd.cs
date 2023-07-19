using Sandbox;
using System;

namespace GGame;

public class TileEventEnd : TileEvent {
    public override string ModelStr { get; set; } = "models/map/endevent.vmdl";

    public TileEventEnd() { }

    public override void Init(Tile tile) {
        base.Init(tile);
        SetupPhysicsFromAABB(PhysicsMotionType.Static, new Vector3(-104, -104, -12), new Vector3(104, 104, 256));
    }

    public override async void Trigger() {
        GGame gam = GGame.Current;
        await gam.AwaitToAndFromBlack();

        Player.Current.Position = Vector3.Zero;
        gam.Score += 500;

        int maxL = Math.Min(gam.currentWorld.length + 1, 12);
        int maxW = Math.Min(gam.currentWorld.width + 1, 10);

        await WorldGen.Current.GenerateWorld(maxL, maxW, gam.currentWorld.depth + 1);

        gam.CurrentDepth = gam.currentWorld.depth;
        Player.Current.Transform = gam.currentWorld.startPos.Add(Vector3.Up * 10, true);
        Player.SetViewAngles(new Angles(0, 0, 0));
        Player.Current.InMenu = false;
    }
}