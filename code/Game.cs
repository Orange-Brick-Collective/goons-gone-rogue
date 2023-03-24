using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GGame;

public partial class GGame : GameManager {
	public static GGame Cur => (GGame)GameManager.Current;

	public WorldManager worldManager;
	public List<Goon> goons = new();

	public GGame() {
		if (Game.IsServer) {
			worldManager = new();
		}

		if (Game.IsClient) {

		}
	}

	public override void Simulate(IClient cl) {
		base.Simulate(cl);

		foreach (Goon goon in goons) {
			goon.SimulateAI();
		}
	}

	public override void ClientJoined(IClient client) {
		base.ClientJoined( client );

		var pawn = new Pawn();
		client.Pawn = pawn;
	}
}
