using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GGame;

public partial class GGame : GameManager {
	public static GGame Cur => (GGame)Current;

	public Transform ArenaMarker {get; set;}
	public World currentWorld;
	public Arena currentArena;
	[Net] public int CurrentDepth {get; set;}
	public Transform currentPosition;

	[Net] public int Score {get; set;} = 0;
	[Net] public int Kills {get; set;} = 0;
	[Net] public int DamageDealt {get; set;} = 0;
	[Net] public int DamageTaken {get; set;} = 0;

	public List<Goon> goons = new();
	
	public GGame() {
		if (Game.IsServer) {
			_ = new WorldGen();
			_ = new ArenaGen();
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
		if (Game.IsServer) {
			var pawn = new Player();
			cl.Pawn = pawn;
			pawn.Transform = ArenaMarker;
		}
	}

	public async void FightOverCheck() {
		foreach (Goon goon in goons) {
			if (goon.Team != 0) return;
		}

		await GameTask.DelayRealtime(500);
		Score += 250;

		ClientFightOverCheck();
	}
	[ClientRpc]
	public void ClientFightOverCheck() {
		Hud._hud.RootPanel.AddChild(new FightEndUI());
	}

	public async void GenSpawnArena() {
		while (true) {
			await GameTask.DelayRealtime(100);

			try {
				ArenaMarker = Entity.All.OfType<ArenaMarker>().First().Transform;
			} catch {}

			if (ArenaMarker != new Transform(Vector3.Zero, new Rotation(0, 0, 0, 0), 0)) break;
		}

		await ArenaGen.Cur.GenerateLevel(0);
	}

	[ConCmd.Server] // password just as a safety to deter console foolery
	public static async void TransitionGameStart(string password) {
		if (password != "dpiol") return;

		await GGame.Cur.TransitionUI();

		await WorldGen.Cur.GenerateLevel(8, 6, 0, false);
		Player.Cur.InMenu = false;
		Player.Cur.Transform = GGame.Cur.currentWorld.startPos;

		Goon g = new();
		g.Init(0, Player.Cur);
		g.Generate(5);
		Goon g2 = new();
		g2.Init(0, Player.Cur);
		g2.Generate(5);
	}

	public async void TransitionGameEnd() {
		await TransitionUI();

		Player.Cur.InMenu = true;
		goons.Clear();
		foreach (var goon in Entity.All.OfType<Goon>()) {
			goon.OnKilled();
		}

		await ArenaGen.Cur.GenerateLevel(WallModels.RandomWall());

		Player.Cur.Transform = ArenaMarker;
		Player.Cur.MaxHealth = 200;
		Player.Cur.Health = 200;
		Player.Cur.Armor = 0;
		Player.Cur.AddMoveSpeed = 0;
		Player.Cur.AddWeaponDamage = 0;
		Player.Cur.AddFireRate = 0;
		Player.Cur.AddReloadTime = 0;
		Player.Cur.AddMagazineSize = 0;
		Player.Cur.AddDegreeSpread = 0;
		Player.Cur.AddRange = 0;
		Player.Cur.CurrentMag = 20;

		Score = 0;
		Kills = 0;
		DamageDealt = 0;
		DamageTaken = 0;
	}

	public async void TransitionLoad() {
		await TransitionUI();


	}

	public async void TransitionStartFight() {
		currentPosition = Player.Cur.Transform;
		await TransitionUI();
		
		await ArenaGen.Cur.GenerateLevel();
		
		// tp player and players goons to place on one side of arena
		Player.Cur.Transform = ArenaMarker.WithPosition(ArenaMarker.Position + new Vector3(-450, 0, 10));
		Player.Cur.IsInCombat = true;
		Player.Cur.CurrentMag = Player.Cur.MagazineSize;
		foreach (Goon goon in goons) {
			if (goon.Team == 0) {
				int x = Random.Shared.Int(-650, -500);
				int y = Random.Shared.Int(-600, 600);
				goon.Position = ArenaMarker.Position + new Vector3(x, y, 10);
				goon.IsInCombat = true;
				goon.CurrentMag = goon.MagazineSize;
			}
		}

		// spawn enemies on other side
		for (int i = 0; i < 2 + Random.Shared.Int(0, currentWorld.depth); i++) {
			Goon g = new();
			g.Init(1);
			g.Generate(currentWorld.depth);
			int x = Random.Shared.Int(500, 650);
			int y = Random.Shared.Int(-600, 600);
			g.Position = ArenaMarker.Position + new Vector3(x, y, 10);
			g.IsInCombat = true;
		}
	}

	public async void TransitionEndFight() {
		await TransitionUI();

		Player.Cur.IsInCombat = false;
		Player.Cur.CurrentMag = Player.Cur.MagazineSize;

		foreach (Goon goon in goons) {
			if (goon.Team == 0) {
				goon.IsInCombat = false;
				goon.CurrentMag = goon.MagazineSize;
			}
		}

		Player.Cur.Transform = currentPosition;
	}

	public async void TransitionLevel() {
		await TransitionUI();

		Score += 500;

		await WorldGen.Cur.GenerateLevel(currentWorld.length + 1, currentWorld.width + 1, currentWorld.depth + 1, false);
		CurrentDepth = currentWorld.depth;
		Player.Cur.Transform = GGame.Cur.currentWorld.startPos;
		Player.Cur.InMenu = false;
	}

	// fades over 600ms
	// stays black 600ms
	// unfades last 600ms --- also dont mark static as vsc wants
	public System.Threading.Tasks.Task TransitionUI() {
		TransitionUIClient();
		return GameTask.DelayRealtime(600);
	}

	[ClientRpc]
	public static void TransitionUIClient() {
		Hud._hud.Loading();
	}
}
