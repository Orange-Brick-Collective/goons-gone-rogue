using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace GGame;

public partial class ShopEntity : ModelEntity, IUse {
    public float rotationSpeed;
    public List<Powerup> powerups = new();

    public ShopEntity Init() {
        Scale = 1.4f;

        List<int> ints = new();
        for (int i = 0; i < System.Random.Shared.Int(3, 6); i++) {
            ints.Add(Powerups.GetRandomIndex);
        }

        foreach (int num in ints) {
            powerups.Add(Powerups.GetByIndex(num));
        }
        ClientInit(ints.ToArray());

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
    public void ClientInit(int[] ints) {
        foreach (int num in ints) {
            powerups.Add(Powerups.GetByIndex(num));
        }
    }

    [GameEvent.Tick.Server]
    private void Tick() {
        Rotation = Rotation.FromYaw(Time.Tick * rotationSpeed);
    }

	public bool OnUse(Entity user) {
        if (Game.IsServer) return true;

        if (!Hud.Current.RootPanel.ChildrenOfType<ShopUI>().Any()) {
            Hud.Current.RootPanel.AddChild(new ShopUI(this, (Pawn)user));
        }
        return true;
	}

	public bool IsUsable(Entity user) {
    	return true;
	}
}