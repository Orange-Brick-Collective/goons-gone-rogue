using Sandbox;
using Sandbox.UI;
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

public partial class Goon : Pawn {
    public GoonState State {get; set;}

    public Player leader;
    public Pawn target;
    public Vector3 posInGroup = Vector3.Zero;

    public bool renderPath = false;
    private Vector3[] path;
    private int currentPath;
    private TimeSince timeSinceHitPath;
    public TimeSince footstep;
    
    // for offsetting occasional ai loop to improve performance
    private int tickCycle = 0;

	public override void Spawn() {
		base.Spawn();
        Tags.Add("goon");

        EnableDrawing = true;

		SetupPhysicsFromAABB(PhysicsMotionType.Keyframed, new Vector3(-16, -16, 0), new Vector3(16, 16, 70));
		SetModel("models/player2.vmdl");

        weapon = new();
        weapon.Init("models/gun.vmdl");
        weapon.Position = Position + new Vector3(8, -12 * Scale, 32 * Scale);
        weapon.Rotation = Rotation;
        weapon.RenderColor = new Color(0.8f, 0.8f, 0.8f);
        weapon.Owner = this;
        weapon.Parent = this;

        RegisterSelf();
	}
	public override void ClientSpawn() {
		base.ClientSpawn();
        if (Team == 0) TeamUI.Add(this);

        RegisterSelf();
	}


	public override void OnKilled() {
		base.OnKilled();
        ClientOnKilled();
        UnregisterSelf();

        if (!Player.Current.InMenu)  {
            if (Team != 0) {
                GGame.Current.Kills += 1;
                GGame.Current.Score += 50;
            }

            GGame.Current.FightOverCheck();
        }

        Delete();
	}

    [ClientRpc]
    public void ClientOnKilled() {
        UnregisterSelf();

        if (Team == 0) TeamUI.Remove(this);
        healthPanel?.Delete();
    }

    public void Init(int team, Player leader = null) {
        Team = team;
        this.leader = leader;
        Tags.Add($"team{team}");

        if (leader is not null) {
            while (Vector3.DistanceBetween(posInGroup, Vector3.Zero) < 50) {
                posInGroup = new Vector3(Random.Shared.Float(-1f, 1f), Random.Shared.Float(-1f, 1f), 0) * 80;
            }
        }
    }

    public void RegisterSelf() {
        GGame.Current.goons.Add(this);
        tickCycle = GGame.Current.goons.Count % 50;
    }
    
    public void UnregisterSelf() {
        GGame.Current.goons.Remove(this);
    }

    // *
    // * AI
    // *

    public void SimulateAI() {
        AiMoveGravity();

        if (IsInCombat) {
            if (target is null || !target.IsValid()) {
                AIFindTarget();
            } else {
                TraceResult tr = Trace.Ray(Position + HeightOffset, target.Position + target.HeightOffset)
                    .Ignore(this)
                    .WithoutTags($"team{Team}")
                    .Run();

                if (Vector3.DistanceBetween(Position, target.Position) > Range || tr.Entity is not Pawn) {
                    AIEngage();
                } else {
                    AIFire();
                }
                
            }
        } else {
            if (leader is null) return;
            AIFollow();
        }
    }

    private void AIFindTarget() {
        State = GoonState.FindingTarget;

        IEnumerable<Entity> e = Entity.FindInSphere(Position, 1500)
            .OfType<Pawn>()
            .Where(g => g.Team != Team)
            .OrderBy(g => Vector3.DistanceBetween(Position, g.Position));

        if (e.Any()) {
            target = (Pawn)e.First();
            AIGeneratePath();
        };
    }

    private void AIEngage() {
        if (target is null || !target.IsValid()) return;

        State = GoonState.Engaging;

        TraceResult tre = Trace.Ray(Position + HeightOffset, target.Position + target.HeightOffset)
            .EntitiesOnly()
            .WithoutTags($"team{Team}")
            .Run();
        AILookat(tre.Direction.WithZ(0));

        if (Time.Tick % 50 == tickCycle) {
            AIFindTarget();
        }

        if (Time.Tick % 500 == tickCycle * 10) {
            AIGeneratePath(); // regen a bit early sometimes for safety
        }

        if (path is null || currentPath >= path.Length) {
            return;
        }
        AIMovePath();
    }

    private void AIFire() {
        State = GoonState.Firing;

        if (Time.Tick % 50 == tickCycle) {
            AIFindTarget();
        }

        TraceResult tre = Trace.Ray(Position + HeightOffset, target.Position + target.HeightOffset)
            .EntitiesOnly()
            .WithoutTags($"team{Team}")
            .Run();
        AILookat(tre.Direction.WithZ(0));

        FireGun(target);
        timeSinceHitPath = 0;
    }

    private void AIFollow() {
        State = GoonState.Following;

        TraceResult tr = Trace.Ray(Position, leader.Position + posInGroup)
            .EntitiesOnly()
            .WithoutTags("goon", "trigger")
            .Run();

        if (tr.Distance > 500) Position = leader.Position + leader.Rotation.Backward * 50 * Vector3.Up * 20;

        if (tr.Distance > 20) {
            AILookat(tr.Direction.WithZ(0));
            AIMoveDirection(tr.Direction.WithZ(0));
        }
    }

    private void AILookat(Vector3 pos) {
        Rotation = Rotation.LookAt(pos);
    }

    // *
    // * moving
    // *

    private void AIMoveDirection(Vector3 forward) {
        MoveHelper help = new(Position, forward * MoveSpeed) {
            Trace = Trace.Body(PhysicsBody, Position)
                .WithoutTags("player", "goon", "trigger"),
        };
        help.TryMoveWithStep(Time.Delta, 20);

        if (Vector3.DistanceBetweenSquared(Position, help.Position) > 1) {
            Position = help.Position;

            if (footstep > 0.36f) {
                footstep = 0;
                PlaySound("sounds/step.sound");
            }
        }
    }

    private void AiMoveGravity() {
        MoveHelper help = new(Position, Vector3.Down * 30) {
            Trace = Trace.Body(PhysicsBody, Position)
                .WithoutTags("player", "goon", "trigger"),
        };

        help.TryMove(Time.Delta);
        Position = help.Position;
    }

    // *
    // * moving path
    // *

    private void AIGeneratePath() {
        try {
            path = NavPathBuilder.Create(Position)
                .WithMaxClimbDistance(4)
                .WithMaxDropDistance(4)
                .WithPartialPaths()
                .Build(target.Position + Vector3.Random.WithZ(0) * 100)
                .Segments
                .Select(x => x.Position)
                .ToArray();
            currentPath = 0;

            if (!renderPath) return;
            for(int i = 0; i < path.Length - 1; i++) {
                Color color = Team == 0 ? Color.Green : Color.Red;
                color = color.WithAlpha(0.4f);
                if (i % 2 == 0) color = color.Darken(0.3f);

                DebugOverlay.Line(path[i], path[i + 1], color, 2, true);
            }
        } catch {
            path = null;
            Log.Warning($"Goon {Name} is stuck or no navmesh");
        }
    }

    private void AIMovePath() {
        MoveHelper help = new(Position, (path[currentPath] - Position).Normal * MoveSpeed) {
            Trace = Trace.Body(PhysicsBody, Position)
                .WithoutTags("player", "goon", "trigger"),
        };

        help.TryMove(Time.Delta);

        if (Vector3.DistanceBetweenSquared(Position, help.Position) > 1) {
            Position = help.Position;

            if (footstep > 0.36f) {
                footstep = 0;
                PlaySound("sounds/step.sound");
            }
        }

        if (timeSinceHitPath > 3) {
            Position = path[currentPath];
            timeSinceHitPath = 0;
        }

        if (Vector3.DistanceBetweenSquared(Position, path[currentPath]) < 400) {
            timeSinceHitPath = 0;
            currentPath++;
        }
    }
}