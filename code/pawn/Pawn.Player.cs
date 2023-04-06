using Sandbox;
using System;

namespace GGame;

public partial class Player : Pawn {
	public static Player Current {get; set;}

	[Net] public bool IsPlaying {get; set;} = false;
	[Net, Change] public bool InMenu {get; set;} = true;

	[ClientInput] public Vector3 InputDirection {get; protected set;}
	[ClientInput] public Angles ViewAngles {get; set;}

	public override void BuildInput() {
		if (!InMenu) {
			Angles viewAngles = ViewAngles;
			viewAngles += Input.AnalogLook;
			viewAngles.pitch = viewAngles.pitch.Clamp(-72, 68);
			ViewAngles = viewAngles.Normal;
		} else {
			ViewAngles = new Angles(20, Time.Tick * 0.16f, 0);
		}

		if (!InMenu && IsPlaying) {
			InputDirection = Input.AnalogMove;
		}
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
        weapon.Position = Position + new Vector3(8, -12 * Scale, 32 * Scale);
        weapon.Rotation = Rotation;
		weapon.RenderColor = new Color(0.8f, 0.8f, 0.8f);
        weapon.Owner = this;
        weapon.Parent = this;

		int random = Random.Shared.Int(hats.Length);
		if (random != hats.Length) {
			_ = new ModelEntity(hats[random], this);
		}

		RenderColor = new Color(0.5f, 0.5f, 0.5f);

		MaxHealth = 200;
		Health = 200;
		BaseWeaponDamage = 10;
	}
	public override void ClientSpawn() {
		Current = this;
		base.ClientSpawn();
	}

	public override void StartTouch(Entity ent) {
		if (!Game.IsServer) return;

		switch (ent) {
			case TileEventFight: {
				ent.Delete();
				GGame.Current.TransitionStartFight();
				break;
			}
			case TileEventEnd: {
				GGame.Current.TransitionLevel();
				break;
			}
		}
	}

	public override void OnKilled() {
		if (InMenu || !IsPlaying) return;
		GGame.Current.TransitionGameEnd();
	}

	public void OnInMenuChanged() {
		if (InMenu) {
			Hud._hud.RootPanel.AddChild(new Menu());
		} else{
			foreach (Menu m in Hud._hud.RootPanel.ChildrenOfType<Menu>()) {
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

		if (Input.Down(InputButton.PrimaryAttack) && Game.IsServer) {
			FireGun();
		}

		if (Input.Pressed(InputButton.Slot1) && Game.IsServer) {
			TraceResult tr = Trace.Ray(Position + HeightOffset, Position + ViewAngles.Forward * 2000).Ignore(this).Run();
			Position = tr.EndPosition;
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
}
