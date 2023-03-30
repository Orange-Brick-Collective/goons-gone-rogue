using Sandbox;

namespace GGame;

public partial class PowerupEntity : ModelEntity {
    public Powerup powerup;
    public float rotationSpeed;

    public PowerupEntity Init(int e) {
        ClientInit(e);
        powerup = new Powerup(Powerups.GetByIndex(e));
        rotationSpeed = System.Random.Shared.Float(-0.3f, 0.3f);

        SetModel("models/powerup.vmdl");
        SetupPhysicsFromModel(PhysicsMotionType.Static);
        return this;
    }

    [ClientRpc] // required
    public void ClientInit(int e) {
        powerup = Powerups.GetByIndex(e);
    }

    [Event.Tick.Server]
    private void Tick() {
        Rotation = Rotation.FromYaw(Time.Tick * rotationSpeed);
    }
}