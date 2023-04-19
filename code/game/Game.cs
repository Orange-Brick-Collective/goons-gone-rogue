using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GGame;

// This game was written like jank. It was a HUGE learning for me (Kodi022) at many
// places in and out of programming, and my future projects will definitely 
// be cleaner as a result. Glad to have been a part of the Game jam.

// ps. this has been fixed up post jam

// ost https://www.youtube.com/watch?v=PB1TqA8JjiA&list=PL1dFsWeZdLiR4ppHRDlk6wiVDc0bIr9PL

public partial class GGame : GameManager {
	public static new GGame Current => (GGame)GameManager.Current;

	public Transform ArenaMarker {get; set;}
	public World currentWorld;
	public Arena currentArena;
	[Net] public int CurrentDepth {get; set;}
	public Transform currentPosition;
	public Angles currentViewAngles;

	// music actually almost gave me a migraine, it wouldnt compile, then it wouldnt work, then it wouldnt .Stop()
	public bool IsMusicEnabled {get; set;} = true;

	[Net] public int Score {get; set;} = 0;
	[Net] public int Kills {get; set;} = 0;
	[Net] public int Powerups {get; set;} = 0;
	[Net] public int DamageDealt {get; set;} = 0;
	[Net] public int DamageTaken {get; set;} = 0;

	public List<Goon> goons = new();
	
	public GGame() {
		if (Game.IsServer) {
			_ = new Leaderboards();
			_ = new WorldGen();
			_ = new ArenaGen();
			_ = new MusicBox();
			GenSpawnArena();
		}

		if (Game.IsClient) {
			_ = new PawnHealth();
			_ = new Hud();
		}
	}

	public override void Simulate(IClient cl) {
		base.Simulate(cl);
		if (Game.IsClient) return;

		for (int i = 0; i < goons.Count; i++) {
			goons[i]?.SimulateAI();
		}
	}

	public override void ClientJoined(IClient cl) {
		base.ClientJoined(cl);

		var pawn = new Player();
		cl.Pawn = pawn;
		pawn.Transform = ArenaMarker;

		MusicBox.Current.SongLooping = Sound.FromScreen("music/explore.sound");
		MusicBox.Current.SongLooping.SetVolume(0);
		MusicBox.Current.LerpToActive("music/menu.sound");

		pawn.controller = new PlayerMenuController(pawn);
	}

	[ConCmd.Server("ggr_kill")]
	public static void ServerKillCMD() {
		Player.Current.OnKilled();
	}

	public void OnIsMusicEnabledChanged() {
		Log.Info("change");
		if (Game.IsClient) return;

		if (IsMusicEnabled) {
			MusicBox.Current.SongLooping = Sound.FromScreen("music/explore.sound");
			MusicBox.Current.LerpToActive("music/menu.sound");
		} else {
			MusicBox.Current?.SongLooping.SetVolume(0);
			MusicBox.Current?.SongActive.SetVolume(0);
		}
	}

	// jic 2 enemies die at same time
	public TimeSince lastFightEnd = 0;
	public async void FightOverCheck() {
		if (Player.Current.InMenu || !Player.Current.IsPlaying) return;
		if (lastFightEnd < 0.1f) return;
		
		foreach (Goon goon in goons) {
			if (goon.Team != 0) return;
		}

		lastFightEnd = 0;
		await GameTask.DelayRealtime(500);

		Score += 250;
		Player.FightEndUI();
	}

	public async void GenSpawnArena() {
		while (true) {
			await GameTask.DelayRealtime(100);

			try {
				ArenaMarker = Entity.All.OfType<ArenaMarker>().First().Transform;
			} catch {}

			if (ArenaMarker != new Transform(Vector3.Zero, new Rotation(0, 0, 0, 0), 0)) break;
		}

		await GameTask.DelayRealtime(200);
		await ArenaGen.Current.GenerateLevel(0);
	}

	[ConCmd.Server] // password just as a safety to deter console foolery
	public static async void GameStart(string password) {
		if (password != "dpiol") return;

		if (Player.Current.IsPlaying) return;

		MusicBox.Current.LerpToLooping();
		await Current.AwaitToAndFromBlack();
		Player.Current.IsPlaying = true;

		await WorldGen.Current.GenerateLevel(8, 6, 0, false);
		Player.Current.InMenu = false;
		Player.Current.Transform = Current.currentWorld.startPos;

		Goon g = new();
		g.Init(0, Player.Current);
		g.Generate(3);
		Goon g2 = new();
		g2.Init(0, Player.Current);
		g2.Generate(3);
	}

	public async void GameEnd() {
		if (!Player.Current.IsPlaying) return;

		Player.Current.IsPlaying = false;

		await AwaitToBlack();

		goons.Clear();
		foreach (var goon in Entity.All.OfType<Goon>()) {
			goon.OnKilled();
		}

		ClientGameEnd();
		MusicBox.Current.LerpToLooping();

		Leaderboards.Current.AddScore(Score);

		await ArenaGen.Current.GenerateLevel(WallModels.RandomWall());
		await GameTask.DelayRealtime(300);

		Player.Current.AppliedPowerups = new Dictionary<string, int>();
		Player.Current.Transform = ArenaMarker;
		Player.Current.MaxHealth = 200;
		Player.Current.Health = 200;
		Player.Current.Armor = 0;
		Player.Current.AddMoveSpeed = 0;
		Player.Current.AddWeaponDamage = 0;
		Player.Current.AddFireRate = 0;
		Player.Current.AddReloadTime = 0;
		Player.Current.AddMagazineSize = 0;
		Player.Current.AddDegreeSpread = 0;
		Player.Current.AddRange = 0;
		Player.Current.CurrentMag = 20;

		Score = 0;
		Kills = 0;
		Powerups = 0;
		DamageDealt = 0;
		DamageTaken = 0;
	}
	[ClientRpc]
	public void ClientGameEnd() {
		TeamUI.Current.Clear();
		Hud.Current.RootPanel.AddChild(new GameEndUI());
	}

	public async void FightStart() {
		if (Player.Current.InMenu || !Player.Current.IsPlaying) return;

		currentPosition = Player.Current.Transform;
		currentViewAngles = Player.Current.ViewAngles;

		Player.Current.IsPlaying = false;
		MusicBox.Current.LerpToActive("music/battle.sound");
		await AwaitToAndFromBlack();
		
		await ArenaGen.Current.GenerateLevel();
		
		// tp player and players goons to one side of arena
		Player.Current.Transform = ArenaMarker.WithPosition(ArenaMarker.Position + new Vector3(-450, 0, 10));
		Player.SetViewAngles(new Angles(0, 0, 0));

		
		Player.Current.IsPlaying = true;
		Player.Current.IsInCombat = true;
		Player.Current.CurrentMag = Player.Current.MagazineSize;

		foreach (Goon goon in goons) {
			if (goon.Team == 0) {
				int x = Random.Shared.Int(-650, -500) + goon.Armor;
				int y = Random.Shared.Int(-600, 600);
				goon.Position = ArenaMarker.Position + new Vector3(x, y, 10);
				goon.IsInCombat = true;
				goon.CurrentMag = goon.MagazineSize;
			}
		}

		// spawn enemies on other side
		for (int i = 0; i < 1 + Random.Shared.Int(Math.Max(currentWorld.depth - 1, 0), currentWorld.depth); i++) {
			Goon goon = new();
			goon.Init(1);
			goon.Generate(currentWorld.depth);
			goon.AddWeaponDamage += (int)(currentWorld.depth * 0.5f);
			int x = Random.Shared.Int(500, 650) - goon.Armor;
			int y = Random.Shared.Int(-600, 600);
			goon.Position = ArenaMarker.Position + new Vector3(x, y, 10);
			goon.IsInCombat = true;
		}
	}

	public async void FightEnd() {
		if (Player.Current.InMenu || !Player.Current.IsPlaying) return;

		MusicBox.Current.LerpToLooping();
		await AwaitToAndFromBlack();

		Player.Current.IsInCombat = false;
		Player.Current.CurrentMag = Player.Current.MagazineSize;

		foreach (Goon goon in goons) {
			if (goon.Team == 0) {
				goon.IsInCombat = false;
				goon.CurrentMag = goon.MagazineSize;
			}
		}

		Player.Current.Transform = currentPosition;
		Player.SetViewAngles(currentViewAngles);		
	}

	public async void ChangeLevel() {
		await AwaitToAndFromBlack();

		Score += 500;

		await WorldGen.Current.GenerateLevel(currentWorld.length + 1, currentWorld.width + 1, currentWorld.depth + 1, false);
		CurrentDepth = currentWorld.depth;
		Player.Current.Transform = GGame.Current.currentWorld.startPos;
		Player.SetViewAngles(new Angles(0, 0, 0));
		Player.Current.InMenu = false;
	}

	// fades over 1s
	// stays black 400ms
	// unfades last 1s --- also dont mark static as vsc wants
	public System.Threading.Tasks.Task AwaitToAndFromBlack() {
		Player.ToAndFromBlack();
		return GameTask.DelayRealtime(1000);
	}

	// fades over 1s
	// stays black --- also dont mark static as vsc wants
	public System.Threading.Tasks.Task AwaitToBlack() {
		Player.ToBlack();
		return GameTask.DelayRealtime(1000);
	}

	// unfades over 1s --- also dont mark static as vsc wants
	public System.Threading.Tasks.Task AwaitFromBlack() {
		Player.FromBlack();
		return GameTask.DelayRealtime(1000);
	}
}
