using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GGame;

// This game was written like jank (during the jam). It was a HUGE learning for me (Kodi022) at many
// places in and out of programming, and my future projects will definitely 
// be cleaner as a result.

// most code has been reworked post jam

// ost https://www.youtube.com/watch?v=PB1TqA8JjiA&list=PL1dFsWeZdLiR4ppHRDlk6wiVDc0bIr9PL

//////////////

// to do maybe
// edit worldgen
// ai control of some sort
// balance boss fight
// balance swarm fight
// fight start action
// tick action
// fix spread to be circular

// ! THIS IS A DEV BRANCH TEST

public partial class GGame : GameManager {
	public static new GGame Current => (GGame)GameManager.Current;

	[Net] public Leaderboard Leaderboard {get; set;}

	public Transform ArenaMarker {get; set;}
	public World currentWorld;
	public Arena currentArena;
	[Net] public int CurrentDepth {get; set;}
	public Transform currentPosition;
	public Angles currentViewAngles;

	public bool IsMusicEnabled {get; set;} = true;

	[Net] public int Money {get; set;} = 0;

	[Net] public int Score {get; set;} = 0;
	[Net] public int Kills {get; set;} = 0;
	[Net] public int Powerups {get; set;} = 0;
	[Net] public int Purchases {get; set;} = 0;
	[Net] public float DamageDealt {get; set;} = 0;
	[Net] public float DamageTaken {get; set;} = 0;

	public List<Goon> goons = new();
	
	public GGame() {
		if (Game.IsServer) {
			Leaderboard = new Leaderboard();
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

		MusicBox.Current.LerpToActive("music/menu.sound");
		MusicBox.Current.SongLooping = Sound.FromScreen("music/explore.sound");
		MusicBox.Current.SongLooping.SetVolume(0);
	}

	[ConCmd.Server("ggr_kill")]
	public static void ServerKillCMD() {
		Player.Current.OnKilled();
	}

	// [ConCmd.Server("ggr_setmoney")]
	// public static void ServerSetMoneyCMD(int money) {
	// 	GGame.Current.Money = money;
	// }

	// [ConCmd.Server("ggr_setdepth")]
	// public static void ServerSetDepthCMD(int depth) {
	// 	GGame.Current.currentWorld.depth = depth;
	// 	GGame.Current.CurrentDepth = depth;
	// }

	public void OnIsMusicEnabledChanged() {
		if (Game.IsClient) return;

		if (IsMusicEnabled) {
			MusicBox.Current?.SongActive.SetVolume(1);
			MusicBox.Current?.SongActive.Stop();
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
		Money += 250;
		Player.FightEndUI();
	}

	public async void GenSpawnArena() {
		while (true) {
			await GameTask.DelayRealtime(200);

			try {
				ArenaMarker = Entity.All.OfType<ArenaMarker>().First().Transform;
			} catch {}

			if (ArenaMarker != new Transform(Vector3.Zero, new Rotation(0, 0, 0, 0), 0)) break;
		}

		await GameTask.DelayRealtime(200);
		await ArenaGen.Current.GenerateArena(0);
	}

	public async void GameStart() {
		if (Player.Current.IsPlaying) return;

		if (IsMusicEnabled) MusicBox.Current.LerpToLooping();
		await Current.AwaitToAndFromBlack();
		Player.Current.IsPlaying = true;

		await WorldGen.Current.GenerateWorld(8, 6, 0);
		Player.Current.InMenu = false;
		Player.Current.Transform = Current.currentWorld.startPos;

		Goon g = new();
		g.Init(0, Player.Current);
		g.Generate(3, Pawn.GoonType.Normal);
		Goon g2 = new();
		g2.Init(0, Player.Current);
		g2.Generate(3, Pawn.GoonType.Normal);
	}

	public async void GameEnd() {
		if (!Player.Current.IsPlaying) return;

		Player.Current.IsPlaying = false;
		Player.Current.IsInCombat = false;

		await AwaitToBlack();

		goons.Clear();
		foreach (var goon in Entity.All.OfType<Goon>()) {
			goon.OnKilled();
		}

		ClientGameEnd();
		if (IsMusicEnabled) MusicBox.Current.LerpToLooping();

		Leaderboard.Current.AddScore(Score);

		await ArenaGen.Current.GenerateArena(WallModels.RandomWall());
		await GameTask.DelayRealtime(300);

		Player.Current.AppliedPowerups = new List<AppliedPowerup>();
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
		GoonStats.UpdatePowerups(Player.Current.NetworkIdent);

		CurrentDepth = 0;
		Money = 0;
		Score = 0;
		Kills = 0;
		Powerups = 0;
		Purchases = 0;
		DamageDealt = 0;
		DamageTaken = 0;
	}
	[ClientRpc]
	public void ClientGameEnd() {
		TeamUI.Current.Clear();
		Hud.Current.RootPanel.AddChild(new GameEndUI());
	}

	public async void FightEnd() {
		if (Player.Current.InMenu || !Player.Current.IsPlaying) return;

		if (IsMusicEnabled) MusicBox.Current.LerpToLooping();
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
