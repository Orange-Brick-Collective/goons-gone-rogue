	using Sandbox;
using System;
using System.Linq;

namespace GGame;

public partial class Player : Pawn {
    [Net, Predicted] public bool IsGrounded {get; set;} = true;

	public void SimulateMovement() {
		TraceResult ground = BoxTrace(new Vector3(32, 32, 16), Position + new Vector3(0, 0, 7.9f)); 
		IsGrounded = ground.Hit;

		// movement input
		Rotation = ViewAngles.WithPitch(0).ToRotation();
		Vector3 input = InputDirection.Normal * Rotation;

		Vector3 newVel = input * 200 + Velocity;

		MoveHelper helper = new(Position, newVel) {
			Trace = Trace.Body(PhysicsBody, Position).WithoutTags("player", "goon", "trigger"),
		};

		if (IsGrounded) {
			helper.ApplyFriction(20, Time.Delta);
			Velocity = helper.Velocity;
		}
		else {
			helper.ApplyFriction(0.2f, Time.Delta);
			Velocity = helper.Velocity * 0.5f - new Vector3(0, 0, 80);
		}
		
		if (helper.TryMoveWithStep(Time.Delta, 50) > 0) {
			Position = helper.Position;
		}
	}
	public static TraceResult BoxTrace(Vector3 extents, Vector3 pos) {
		return Trace.Box(extents, pos, pos).WithoutTags("player", "goon", "trigger").Run();
	}
	public static TraceResult BoxTraceSweep(Vector3 extents, Vector3 from, Vector3 to) {
		return Trace.Box(extents, from, to).WithoutTags("player", "goon", "trigger").Run();
    }
}