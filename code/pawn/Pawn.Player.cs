﻿using Sandbox;
using System;

namespace GGame;

public partial class Player : Pawn {
	public static Pawn Cur {get; set;}

	[Net, Change] public bool IsActive {get; set;} = false;

	[ClientInput] public Vector3 InputDirection {get; protected set;}
	[ClientInput] public Angles ViewAngles {get; set;}

	public override void BuildInput() {
		InputDirection = Input.AnalogMove;

		Angles look = Input.AnalogLook;

		Angles viewAngles = ViewAngles;
		viewAngles += look;
        viewAngles.pitch = viewAngles.pitch.Clamp(-72, 68);
		ViewAngles = viewAngles.Normal;
	}

	public override void Spawn() {
		base.Spawn();
		Cur = this;

		Tags.Add("player");
		Tags.Add("team0");
		Name = "Player";
		
		EnableTouch = true;
		EnableDrawing = true;

		SetModel("models/player2.vmdl");
		SetupPhysicsFromAABB(PhysicsMotionType.Keyframed, new Vector3(-16, -16, 0), new Vector3(16, 16, 70));
        
		weapon = new();
        weapon.Init("models/gun.vmdl");
        weapon.Position = Position + new Vector3(0, -12 * Scale, 35 * Scale);
        weapon.Rotation = Rotation;
        weapon.Owner = this;
        weapon.Parent = this;
	}
	public override void ClientSpawn() {
		Cur = this;
		base.ClientSpawn();
	}

	public override void StartTouch(Entity ent) {
		switch (ent) {
			case TileEventFight: {
				Log.Info("touch fight");
				break;
			}
			case TileEventEnd: {

				break;
			}
		}
	}

	public override void OnKilled() {
		// game over

		// back to menu
	}

	public void OnIsActiveChanged() {
		if (IsActive) {
			EnableDrawing = true;
			EnableAllCollisions = true;
		}
	}

	// *
	// * SIMULATES
	// *

	public override void Simulate(IClient cl) {
		base.Simulate(cl);

		SimulateMovement();
		SimulateUse();

		if (Input.Down(InputButton.PrimaryAttack) && Game.IsServer) {
			FireGun();
		}

		if (Input.Pressed(InputButton.Slot1) && Game.IsServer) {
			TraceResult tr = Trace.Ray(Camera.Position, Camera.Position + Camera.Rotation.Forward * 4000).Ignore(this).Run();
			Goon g = new();
			g.Init(0, this);
			g.Position = tr.EndPosition + Vector3.Up * 5;
		}

		if (Input.Pressed(InputButton.Slot2) && Game.IsServer) {
			TraceResult tr = Trace.Ray(Camera.Position, Camera.Position + Camera.Rotation.Forward * 4000).Ignore(this).Run();
			Goon g = new();
			g.Init(1);
			g.Position = tr.EndPosition + Vector3.Up * 5;

			float rHP = Random.Shared.Float(50, 400);
			g.MaxHealth = rHP;
			g.Health = rHP;

			float rScale = Random.Shared.Float(0.4f, 2f);
			g.Scale = rScale;
		}

		if (Input.Pressed(InputButton.Reload) && Game.IsServer) {
			TraceResult tr = Trace.Ray(Camera.Position, Camera.Position + Camera.Rotation.Forward * 400).Ignore(this).Run();
			PowerupEntity p = new();
			p.Init(Powerups.GetRandomIndex);
			p.Position = tr.EndPosition + Vector3.Up * 50;
		}

		if (Input.Pressed(InputButton.Slot4) && Game.IsServer) {
			TraceResult tr = Trace.Ray(Camera.Position, Camera.Position + Camera.Rotation.Forward * 8000).Ignore(this).Run();
			Position = tr.EndPosition + tr.Normal * 50;
		}
	}

	public override void FrameSimulate(IClient cl) {
		base.FrameSimulate( cl );
		SimulateCamera();
	}

	public void SimulateCamera() {
		Vector3 pos = Position;
		pos += Rotation.Right * 25;
		pos.z += 60;

		// back
		TraceResult tr = Trace.Ray(pos, pos - (ViewAngles.Forward * 60)).WithoutTags("player", "goon", "trigger").Run();
		pos = tr.EndPosition - tr.Direction * 15;
		
		Camera.Position = pos;
		Camera.Rotation = ViewAngles.ToRotation();

		Camera.FieldOfView = Screen.CreateVerticalFieldOfView(Game.Preferences.FieldOfView);
	}
}
