using Sandbox;
using System;

namespace GGame;

public class TileEventBoss : TileEvent {
    public override string ModelStr {get; set;} = "models/map/bossevent.vmdl";

    public TileEventBoss() {
        RenderColor = new Color(0.5f, 0.3f, 0.3f);
    }

    public override async void Trigger() {
        if (Player.Current.InMenu || !Player.Current.IsPlaying) return;

        Player plr = Player.Current;
        GGame gam = GGame.Current;

		gam.currentPosition = plr.Transform;
		gam.currentViewAngles = plr.ViewAngles;

		plr.IsPlaying = false;
		if (gam.IsMusicEnabled) MusicBox.Current.LerpToActive("music/battle.sound");
		await gam.AwaitToAndFromBlack();
		
		await ArenaGen.Current.GenerateArena();
		
		// tp player and players goons to one side of arena
		plr.Transform = gam.ArenaMarker.WithPosition(gam.ArenaMarker.Position + new Vector3(-450, 0, 10));
		Player.SetViewAngles(new Angles(0, 0, 0));
		
		plr.IsPlaying = true;
		plr.IsInCombat = true;
		plr.CurrentMag = plr.MagazineSize;

		foreach (Goon goon in gam.goons) {
			if (goon.Team == 0) {
				int x = Random.Shared.Int(-650, -500) + goon.Armor;
				int y = Random.Shared.Int(-600, 600);
				goon.Position = gam.ArenaMarker.Position + new Vector3(x, y, 10);
				goon.IsInCombat = true;
				goon.CurrentMag = goon.MagazineSize;
			}
		}

		// spawn boss on other side
        Goon goone = new();
        goone.Init(1);
        goone.Generate(gam.currentWorld.depth + 12);

        goone.Scale = 1.5f + gam.currentWorld.depth * 0.1f;
        goone.SetupPhysicsFromAABB(PhysicsMotionType.Keyframed, new Vector3(-16, -16, 0), new Vector3(16, 16, 76));
        goone.MaxHealth += gam.currentWorld.depth * 25;
        goone.Health += gam.currentWorld.depth * 25;

        int xx = Random.Shared.Int(500, 650);
        int yy = Random.Shared.Int(-600, 600);
        goone.Position = gam.ArenaMarker.Position + new Vector3(xx, yy, 10);
        goone.IsInCombat = true;

        Delete();
    }
}