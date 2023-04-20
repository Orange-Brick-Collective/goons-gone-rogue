using Sandbox;
using System;

namespace GGame;

public partial class Player : Pawn {
	public static Player Current {get; set;}

	public PlayerController controller;

	[Net, Change] public bool IsPlaying {get; set;} = false;
	[Net, Change] public bool InMenu {get; set;} = true;

	[ClientInput] public Vector3 InputDirection {get; set;}
	[ClientInput] public Angles ViewAngles {get; set;}

	public override void BuildInput() {
		controller?.BuildInput();
	}

	public override void Spawn() {
		base.Spawn();
		Current = this;

		Tags.Add("player");
		Tags.Add("team0");
		Name = "Player";
		
		EnableTouch = true;
		EnableDrawing = true;

		SetModel("models/player2.vmdl");
		SetupPhysicsFromAABB(PhysicsMotionType.Keyframed, new Vector3(-16, -16, 0), new Vector3(16, 16, 70));
        
		weapon = new();
        weapon.Init("models/gun.vmdl");
        weapon.Position = Position + new Vector3(13, -12 * Scale, 34 * Scale);
        weapon.Rotation = Rotation;
		weapon.RenderColor = new Color(0.8f, 0.8f, 0.8f);
        weapon.Owner = this;
        weapon.Parent = this;

		int random = Random.Shared.Int(hats.Length);
		if (random != hats.Length) {
			_ = new ModelEntity(hats[random], this);
		}

		RenderColor = new Color(0.6f, 0.6f, 0.6f);

		controller = new PlayerMenuController(this);

		MaxHealth = 200;
		Health = 200;
		BaseWeaponDamage = 8;
	}
	public override void ClientSpawn() {
		Current = this;
		controller = new PlayerMenuController(this);
		base.ClientSpawn();
	}

	public override void StartTouch(Entity ent) {
		if (!Game.IsServer) return;

		switch (ent) {
			case TileEventFight: {
				ent.Delete();
				GGame.Current.FightStart();
				break;
			}
			case TileEventEnd: {
				GGame.Current.ChangeLevel();
				break;
			}
		}
	}

	public override void OnKilled() {
		if (InMenu || !IsPlaying) return;
		GGame.Current.GameEnd();
	}

	public void OnIsPlayingChanged() {
		if (IsPlaying) {
			controller = new PlayerPlayingController(this);
		}
	}

	public void OnInMenuChanged() {
		if (InMenu) {
			controller = new PlayerMenuController(this);
			Hud.Current.RootPanel.AddChild(new Menu());
		} else{
			foreach (Menu m in Hud.Current.RootPanel.ChildrenOfType<Menu>()) {
				m.Delete();
			}
		}
	}

	// *
	// * SIMULATES
	// *

	public override void Simulate(IClient cl) {
		base.Simulate(cl);
		if (InMenu || !IsPlaying) return;

		SimulateMovement();
		SimulateUse();

		if (Input.Down("attack1") && Game.IsServer) {
			FireGun();
		}
	}

	public override void FrameSimulate(IClient cl) {
		base.FrameSimulate( cl );
		SimulateCamera();
	}

	public void SimulateCamera() {
		Vector3 pos = Position;
		if (!InMenu) {
			pos.z += 60;

			TraceResult trSide = Trace.Ray(pos, pos - (Rotation.Left * 40))
				.WithoutTags("player", "goon", "trigger")
				.Run();
			pos = trSide.EndPosition - trSide.Direction * 15;

			TraceResult trBack = Trace.Ray(pos, pos - (ViewAngles.Forward * 60))
				.WithoutTags("player", "goon", "trigger")
				.Run();
			pos = trBack.EndPosition - trBack.Direction * 15;
		} else {
			pos.z += 40;
			pos -= ViewAngles.Forward * 100;
		}
		
		Camera.Position = pos;
		Camera.Rotation = ViewAngles.ToRotation();

		Camera.FieldOfView = Screen.CreateVerticalFieldOfView(86);
	}

	// *
	// * RPC
	// *
	[ClientRpc]
	public static void FightEndUI() {
		Hud.Current.RootPanel.AddChild(new FightEndUI());
	}

	[ClientRpc]
	public static void SetViewAngles(Angles angle) {
		Current.ViewAngles = angle;
	}

	[ClientRpc]
	public static async void ToAndFromBlack() {
		Hud.Current.ToBlack();
		await GameTask.DelayRealtime(1400);
		Hud.Current.FromBlack();
	}

	[ClientRpc]
	public static void ToBlack() {
		Hud.Current.ToBlack();
	}
	
	[ClientRpc]
	public static void FromBlack() {
		Hud.Current.FromBlack();
	}

	[ClientRpc]
	public static void FloatingText(Vector3 position, float damage) {
		Hud.Current.floatingText.Create(position)
			.WithText($"{damage:#0.00}")
			.WithLifespan(1)
			.WithMotion(Vector2.Up, 100, 0, 0);
	}
}
