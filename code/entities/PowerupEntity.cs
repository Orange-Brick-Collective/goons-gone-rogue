using System.Linq;
using Sandbox;

namespace GGame;

public partial class PowerupEntity : ModelEntity, IUse {
    public Powerup powerup;
    public float rotationSpeed;

    public PowerupEntity Init(int e) {
        ClientInit(e);
        powerup = Powerups.GetByIndex(e);
        rotationSpeed = System.Random.Shared.Float(-0.3f, 0.3f);

        SetModel("models/powerup.vmdl");
        SetupPhysicsFromModel(PhysicsMotionType.Static);

        _ = new PointLightEntity() {
            Color = new Color(0.4f, 0.4f, 1f),
            Range = 64,
            Brightness = 0.5f,
            Parent = this,
            Transform = this.Transform,
        };

        return this;
    }

    [ClientRpc] // required
    public void ClientInit(int e) {
        powerup = Powerups.GetByIndex(e);
    }

    [GameEvent.Tick.Server]
    private void Tick() {
        Rotation = Rotation.FromYaw(Time.Tick * rotationSpeed);
    }

	public bool OnUse(Entity user) {
        if (Game.IsServer) return true;

        if (!Hud.Current.RootPanel.ChildrenOfType<PowerupUI>().Any()) {
            Hud.Current.RootPanel.AddChild(new PowerupUI(this, (Player)user));
        }
        return true;
	}

	public bool IsUsable(Entity user) {
		return true;
	}
}