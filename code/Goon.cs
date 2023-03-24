using Sandbox;
using System;
using System.Collections.Generic;

namespace GGame;

public class Goon : AnimatedEntity {
    public bool isInCombat = false;
    public int team = 0;
    
    public Pawn leader;
    public Entity target;
    public Vector3 posInGroup = Vector3.Zero;

    int maxHealth = 100;
    int health = 100;

	public override void Spawn() {
		base.Spawn();
		Tags.Add("goon");
		
		SetupPhysicsFromAABB(PhysicsMotionType.Keyframed, new Vector3(-10, -10, 0), new Vector3(10, 10, 50));
		SetModel("models/player.vmdl");

		EnableDrawing = true;
        Scale = 0.7f;
	}

	public override void OnKilled() {
		base.OnKilled();
        UnregisterSelf();
        Delete();
	}

    public void Init(int team, Pawn leader = null) {
        this.team = team;
        this.leader = leader;

        if (leader is not null) {
            while (Vector3.DistanceBetween(posInGroup, Vector3.Zero) < 50) {
                posInGroup = new Vector3(Random.Shared.Float(-1f, 1f), Random.Shared.Float(-1f, 1f), 0) * 80;
            }
        }

        RegisterSelf();
    }

    public void RegisterSelf() {
        GGame.Cur.goons.Add(this);
    }
    
    public void UnregisterSelf() {
        GGame.Cur.goons.Remove(this);
    }

    public void SimulateAI() {
        if (isInCombat) {
            if (target is null) {
                AIEngage();
                return;
            } else {

            }
        } else {
            if (leader is null) return;
            AIFollow();
        }
    }

    private void AIEngage() {

    }

    private void AIFire() {

    }

    private void AIFollow() {
        TraceResult tr = Trace.Ray(Position, leader.Position + posInGroup).EntitiesOnly().WithoutTags("goon").Run();

        if (tr.Distance > 500) Position = leader.Position + leader.Rotation.Backward * 50;

        if (tr.Distance > 20) {
            Rotation = Rotation.From(Vector3.VectorAngle(tr.Direction.WithZ(0)));

            MoveHelper help = new(Position, Velocity + Rotation.Forward * 220 + Rotation.Down * 100) {
                Trace = Trace.Box(new Vector3(16, 16, 0) * Scale, Position, Position).WithoutTags("player", "goon"),
            };
            
            help.TryMove(Time.Delta);
            Position = help.Position;
        }
    }
}