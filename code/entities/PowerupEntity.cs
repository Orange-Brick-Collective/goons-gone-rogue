using System.Linq;
using Sandbox;
using Sandbox.UI;

namespace GGame;

public partial class PowerupEntity : ModelEntity, IUse {
    public Powerup powerup;
    public float rotationSpeed;
    public WorldPanel panel;

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
    [ClientRpc]
    public void ClientInit(int e) {
        powerup = Powerups.GetByIndex(e);

        panel = new WorldPanel() {
            Transform = this.Transform,
            PanelBounds = new Rect(-12, -12, 24, 24),
            WorldScale = 14,
        };
        panel.StyleSheet.Add(StyleSheet.FromFile("ui/PowerupEntityImage.scss"));

        panel.AddChild(new Image() {
            Texture = Texture.Load(FileSystem.Mounted, powerup.Image),
            Classes = "",
        });
    }

    [GameEvent.Tick.Server]
    private void Tick() {
        Rotation = Rotation.FromYaw(Time.Tick * rotationSpeed);
    }

    [GameEvent.Tick.Client]
    private void ClientTick() {
        panel.Rotation = Rotation;
    }

	public bool OnUse(Entity user) {
        if (Game.IsServer) return true;

        if (!Hud.Current.RootPanel.ChildrenOfType<PowerupUI>().Any()) {
            Hud.Current.RootPanel.AddChild(new PowerupUI(this, (Pawn)user));
        }
        return true;
	}

	public bool IsUsable(Entity user) {
		return true;
	}

    protected override void OnDestroy() {
        ClientOnDestroy();
        base.OnDestroy();
    }

    [ClientRpc]
    private void ClientOnDestroy() {
        panel?.Delete();
    }
}
