using System.Linq;
using Sandbox;

namespace GGame;

public partial class ShopEntity : ModelEntity, IUse {
    public float rotationSpeed;

    public ShopEntity Init() {
        Scale = 1.4f;
        ClientInit();
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
    public void ClientInit() {

    }

    [GameEvent.Tick.Server]
    private void Tick() {
        Rotation = Rotation.FromYaw(Time.Tick * rotationSpeed);
    }

	public bool OnUse(Entity user) {
        if (!Hud.Current.RootPanel.ChildrenOfType<ShopUI>().Any()) {
            Hud.Current.RootPanel.AddChild(new ShopUI(this, (Player)user));
        }
        return true;
	}

	public bool IsUsable(Entity user) {
    	return true;
	}
}