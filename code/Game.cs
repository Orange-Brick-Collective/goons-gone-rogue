using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GGame;

public partial class GGame : GameManager {
	public static GGame Cur => (GGame)GameManager.Current;

	public List<Goon> goons = new();

	public GGame() {
		if (Game.IsServer) {
			_ = new WorldManager();
		}

		if (Game.IsClient) {
			_ = new PawnHealth();
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
		pawn.Position += new Vector3(-30, 0, 0);
	}
}
