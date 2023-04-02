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

	[Net] public int Points {get; set;} = 0;

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
		Points += 250;

		ClientFightOverCheck();
	}
	[ClientRpc]
	public void ClientFightOverCheck() {
		Hud._hud.RootPanel.AddChild(new FightEndUI());
	}

	public async void GenSpawnArena() {
		await GameTask.DelayRealtime(2000); // delay because map isnt loaded when game starts...
		ArenaMarker = Entity.All.OfType<ArenaMarker>().First().Transform;
		await ArenaGen.Cur.GenerateLevel(0);
	}

	[ConCmd.Server] // password just as a safety to deter console foolery
	public static async void TransitionStart(string password) {
		if (password != "dpiol") return;

		await GGame.Cur.TransitionUI();

		await WorldGen.Cur.GenerateLevel(7, 5, 0, false);
		Player.Cur.InMenu = false;
		Player.Cur.Transform = GGame.Cur.currentWorld.startPos;
		Goon g = new();
		g.Init(0, Player.Cur);
		g.Generate(5);
		Goon g2 = new();
		g2.Init(0, Player.Cur);
		g2.Generate(5);
	}

	public async void TransitionEnd() {
		await TransitionUI();
		
		Player.Cur.InMenu = true;
		Player.Cur.Transform = ArenaMarker;
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

		Points += 500;

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
