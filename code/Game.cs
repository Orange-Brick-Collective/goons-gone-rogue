using Sandbox;
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

		}
	}

	public override void Simulate(IClient cl) {
		base.Simulate(cl);
		for (int i = 0; i < goons.Count; i++) {
			goons[i]?.SimulateAI();
		}
	}

	public override void ClientJoined(IClient client) {
		base.ClientJoined( client );

		var pawn = new Pawn();
		client.Pawn = pawn;
	}
}
