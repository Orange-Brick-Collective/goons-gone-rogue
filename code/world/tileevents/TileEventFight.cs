using Sandbox;
using System;

namespace GGame;

public class TileEventFight : TileEvent {
    public override string ModelStr {get; set;} = "models/map/enemyevent.vmdl";

    public TileEventFight() {
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

		// spawn enemies on other side
		for (int i = 0; i < 1 + gam.currentWorld.depth; i++) {
			Goon goon = new();
			goon.Init(1);
			goon.Generate(gam.currentWorld.depth, Pawn.GoonType.Normal);
			goon.AddWeaponDamage += (int)(gam.currentWorld.depth * 0.5f);
			int x = Random.Shared.Int(500, 650) - goon.Armor;
			int y = Random.Shared.Int(-600, 600);
			goon.Position = gam.ArenaMarker.Position + new Vector3(x, y, 10);
			goon.IsInCombat = true;
		}

        Delete();
    }
}