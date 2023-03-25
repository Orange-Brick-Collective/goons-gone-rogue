using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GGame;

public enum GoonState {
    Idle,
    Following,
    FindingTarget,
    Engaging,
    Firing,
}

public class Goon : AnimatedEntity {
    public GoonState State {get; set;}
    public bool isInCombat = true;
    public int team = 0;
    
    public Pawn leader;
    public Entity target;
    public Vector3 posInGroup = Vector3.Zero;

    public float maxHealth = 100;
    public new float Health = 100;

    public int moveSpeed = 200;
    public float fireRate = 0.2f;
    public int weaponDamage = 10;

    public Gun weapon;
    public TimeSince lastFire;

    private Vector3[] path;
    private int currentPath;
    
    // for updating occasional things like target
    private int tickCycle = 0;

	public override void Spawn() {
		base.Spawn();
        Tags.Add("goon");

        Scale = 0.6f;
        EnableDrawing = true;

		SetupPhysicsFromAABB(PhysicsMotionType.Keyframed, new Vector3(-16, -16, 0), new Vector3(16, 16, 70));
		SetModel("models/player.vmdl");

        weapon = new();
        weapon.Init("models/testgun.vmdl");
        weapon.Position = Position + new Vector3(0, 16 * Scale, 35 * Scale);
        weapon.Rotation = Rotation;
        weapon.Owner = this;
        weapon.Parent = this;
	}

	public override void TakeDamage( DamageInfo info ) {
        if (Health <= 0) OnKilled();
        else Health -= info.Damage;
	}

	public override void OnKilled() {
		base.OnKilled();
        UnregisterSelf();
        Delete();
	}

    public void Init(int team, Pawn leader = null) {
        this.team = team;
        this.leader = leader;
        Tags.Add($"team{team}");

        if (leader is not null) {
            RenderColor = Color.Green;
            while (Vector3.DistanceBetween(posInGroup, Vector3.Zero) < 50) {
                posInGroup = new Vector3(Random.Shared.Float(-1f, 1f), Random.Shared.Float(-1f, 1f), 0) * 80;
            }
        }

        RegisterSelf();
    }

    public void RegisterSelf() {
        GGame.Cur.goons.Add(this);
        tickCycle = GGame.Cur.goons.Count % 25;
    }
    
    public void UnregisterSelf() {
        GGame.Cur.goons.Remove(this);
    }

    public void SimulateAI() {
        if (isInCombat) {
            if (target is null || !target.IsValid()) {
                AIFindTarget();
                return;
            } else {
                AIEngage();
            }
        } else {
            if (leader is null) return;
            AIFollow();
        }
    }

    private void AIFindTarget() {
        State = GoonState.FindingTarget;

        IEnumerable<Entity> e;
        if (team == 0) {
            e = Entity.FindInSphere(Position, 5000)
                .OfType<Goon>()
                .Where(g => g.team != team)
                .OrderBy(g => Vector3.DistanceBetween(Position, g.Position));
        } else {
            e = Entity.FindInSphere(Position, 5000)
                .Where(g => /*g is Pawn ||*/ g is Goon a && a.team != team)
                .OrderBy(g => Vector3.DistanceBetween(Position, g.Position));
        }
        if (e.Any()) target = e.First();
    }

    private void AIEngage() {
        if (target is null || !target.IsValid()) return;
        
        TraceResult tr = Trace.Ray(Position + Vector3.Up * 30, target.Position)
            .Ignore(this)
            .WithoutTags($"team{team}")
            .Run();

        if (Vector3.DistanceBetween(Position, target.Position) > 400 && tr.Entity is not Goon) {
            State = GoonState.Engaging;

            TraceResult tre = Trace.Ray(Position + Vector3.Up * 30, target.Position)
                .EntitiesOnly()
                .WithoutTags($"team{team}")
                .Run();
            AILookat(tre.Direction.WithZ(0));

            if (path is null || currentPath == path.Length) AIGeneratePath();
            AIMovePath();

            if (Time.Tick % 25 == tickCycle) {
                AIFindTarget();
            }
        } else {
            State = GoonState.Firing;

            AIAttack();
        }
    }

    private void AIAttack() {
        TraceResult tr = Trace.Ray(Position, target.Position)
            .WithoutTags($"team{team}")
            .Run();
        AILookat(tr.Direction);

        if (lastFire > fireRate) {
            lastFire = 0;
            weapon.Fire(this, weaponDamage, () => {});
        } 
    }

    private void AIFollow() {
        State = GoonState.Following;

        TraceResult tr = Trace.Ray(Position, leader.Position + posInGroup)
            .EntitiesOnly()
            .WithoutTags("goon")
            .Run();

        if (tr.Distance > 500) Position = leader.Position + leader.Rotation.Backward * 50;

        if (tr.Distance > 20) {
            AILookat(tr.Direction.WithZ(0));
            AIMoveDirection(tr.Direction.WithZ(0));
        }
    }

    // *
    // * looking
    // *

    private void AILookat(Vector3 pos) {
        Rotation = Rotation.LookAt(pos);
    }

    // *
    // * moving
    // *

    private void AIMoveDirection(Vector3 forward) {
        MoveHelper help = new(Position, forward * moveSpeed) {
            Trace = Trace.Box(new Vector3(16, 16, 0) * Scale, Position, Position)
                .WithoutTags("player", "goon"),
        };
        
        help.TryMove(Time.Delta);
        Position = help.Position;
    }

    private void AiMoveGravity() {
        MoveHelper help = new(Position, Vector3.Down * 400) {
            Trace = Trace.Box(new Vector3(16, 16, 0) * Scale, Position, Position)
                .WithoutTags("player", "goon"),
        };
        
        help.TryMove(Time.Delta);
        Position = help.Position;
    }

    // *
    // * moving path
    // *

    private void AIGeneratePath() {
        path = NavMesh.BuildPath(Position, target.Position);
        currentPath = 0;
    }
    private void AIMovePath() {
        if (path is null || currentPath == path.Length) {
            AIGeneratePath();
            return;
        }

        MoveHelper help = new(Position, (path[currentPath] - Position).Normal * moveSpeed) {
            Trace = Trace.Box(new Vector3(16, 16, 0) * Scale, Position, Position)
                .WithoutTags("player", "goon"),
        };
            
        help.TryMoveWithStep(Time.Delta, 16);
        Position = help.Position;

        if (Vector3.DistanceBetween(Position, path[currentPath]) < 20) {
            currentPath++;
        }
    }
}