using Sandbox;
using System;
using System.Linq;

namespace GGame;

public partial class Pawn : AnimatedEntity {
	[Net, Change] public bool IsActive {get; set;} = false;

	[ClientInput] public Vector3 InputDirection {get; protected set;}
	[ClientInput] public Angles ViewAngles {get; set;}

	public override void BuildInput() {
		InputDirection = Input.AnalogMove;

		Angles look = Input.AnalogLook;

		Angles viewAngles = ViewAngles;
		viewAngles += look;
        viewAngles.pitch = viewAngles.pitch.Clamp(-86, 86);
		ViewAngles = viewAngles.Normal;
	}

	public override void Spawn() {
		base.Spawn();
		Tags.Add("player");
		
		SetupPhysicsFromAABB(PhysicsMotionType.Keyframed, new Vector3(-16, -16, 0), new Vector3(16, 16, 70));
		SetModel("models/player.vmdl");

		EnableDrawing = true;
	}

	public override void OnKilled() {
		// game over

		// back to menu
	}

	public void OnEnemyKilled() {
		// check boosts
	}

	public void OnIsActiveChanged() {
		if (IsActive) {
			EnableDrawing = true;
			EnableAllCollisions = true;
		}
	}

	// *
	// * SIMULATES AND GAMEMODE VARS
	// *

	[Net] public bool IsInCombat {get; set;} = false;

	[Net] public int BaseAttackDamage {get; set;} = 20;
	[Net] public int AddedAttackDamage {get; set;} = 0;

	[Net] public float BaseAttackRate {get; set;} = 0.5f;
	[Net] public float AddedAttackRate {get; set;} = 0;

	public int AttackDamage => BaseAttackDamage + AddedAttackDamage;
	public float AttackRate => BaseAttackRate + AddedAttackRate; // second time

	public override void Simulate(IClient cl) {
		base.Simulate(cl);

		SimulateMovement();

		if (Input.Pressed(InputButton.PrimaryAttack) && Game.IsServer) {
			TraceResult tr = Trace.Ray(Camera.Position, Camera.Position + Camera.Rotation.Forward * 400).Ignore(this).Run();
			Goon g = new();
			g.Init(0, this);
			g.Position = tr.EndPosition;
		}

		if (Input.Pressed(InputButton.SecondaryAttack) && Game.IsServer) {
			TraceResult tr = Trace.Ray(Camera.Position, Camera.Position + Camera.Rotation.Forward * 400).Ignore(this).Run();
			Goon g = new();
			g.Init(1);
			g.Position = tr.EndPosition;
		}
	}

	public override void FrameSimulate(IClient cl) {
		base.FrameSimulate( cl );

		SimulateCamera();
	}

	[Net, Predicted] public bool IsGrounded {get; set;} = true;

	public void SimulateMovement() {
		// grounded and stepup/down
		TraceResult ground;
		if (IsGrounded) {
			Vector3 offset = new(0, 0, 18);
			ground = BoxTraceSweep(new Vector3(32, 32, 4), Position + offset, Position - offset);
			if (ground.Hit) Position = Position.WithZ(ground.HitPosition.z);
		} else {
			ground = BoxTrace(new Vector3(32, 32, 16), Position + new Vector3(0, 0, 7.9f));
		}
		IsGrounded = ground.Hit;

		// movement input
		Rotation = ViewAngles.WithPitch(0).ToRotation();
		Vector3 input = InputDirection.Normal * Rotation;

		Vector3 newVel = input * 200 + Velocity;

		if (!IsGrounded) {
			newVel.z -= 800;
		}

		MoveHelper helper = new(Position, newVel) {
			Trace = Trace.Box(new Vector3(32, 32, 0), Position, Position).WithoutTags("player", "goon"),
		};
		helper.ApplyFriction(20, Time.Delta);

		Velocity = helper.Velocity;
		if (helper.TryMove(Time.Delta) > 0) {
			Position = helper.Position;
		}
	}
	public TraceResult BoxTrace(Vector3 extents, Vector3 pos) {
		return Trace.Box(extents, pos, pos).WithoutTags("player", "goon").Run();
	}
	public TraceResult BoxTraceSweep(Vector3 extents, Vector3 from, Vector3 to) {
		return Trace.Box(extents, from, to).WithoutTags("player", "goon").Run();
	}

	public void SimulateCamera() {
		Vector3 pos = Position;
		pos += Rotation.Right * 20;
		pos.z += 58;
		// back
		pos = Trace.Ray(pos, pos - (ViewAngles.Forward * 60)).WithoutTags("player", "goon").Run().EndPosition;
		
		Camera.Position = pos;
		Camera.Rotation = ViewAngles.ToRotation();

		Camera.FieldOfView = Screen.CreateVerticalFieldOfView(Game.Preferences.FieldOfView);
	}
}
