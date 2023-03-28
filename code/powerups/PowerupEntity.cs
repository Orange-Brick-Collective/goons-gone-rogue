using Sandbox;

namespace GGame;

public partial class PowerupEntity : ModelEntity {
    public Powerup powerup;

    public void Init(int e) {
        this.powerup = Powerups.GetByIndex(e);
        ClientInit(e);
        SetModel("models/powerup.vmdl");
        SetupPhysicsFromSphere(PhysicsMotionType.Static, Position, 12);
    }

    [ClientRpc] // required
    public void ClientInit(int e) {
        this.powerup = Powerups.GetByIndex(e);
    }

    [Event.Tick.Server]
    public void Tick() {
        Rotation = Rotation.FromYaw(Time.Tick * 0.2f);
    }
}