using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace GGame;

public partial class GGame : GameManager {
	public static GGame Cur => (GGame)Current;

	public Transform ArenaMarker {get; set;}
	public World currentWorld;
	public Arena currentArena;

	public List<Goon> goons = new();
	public int points = 0;

	public GGame() {
		if (Game.IsServer) {
			_ = new WorldGen();
			_ = new ArenaGen();

			GetArenaPos();
		}

		if (Game.IsClient) {
			_ = new PawnHealth();
			_ = new Hud();
		}
	}

	public async void GetArenaPos() {
		await GameTask.DelayRealtime(1000);
		ArenaMarker = Entity.All.OfType<ArenaMarker>().First().Transform;
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
	}

	public async void TransitionLoad() {

	}

	public async void TransitionFight() {
		ArenaGen.Cur.GenerateLevel();

	}

	public async void TransitionLevel() {
		
	}

	public async void TransitionUI() {

	}
}
