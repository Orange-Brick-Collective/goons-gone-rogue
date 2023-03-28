using Sandbox;
using System.Linq;

namespace GGame;

public partial class Player : Pawn {
    public Entity hitEnt;
    
    public void SimulateUse() {
        TraceResult useTrace = Trace.Ray(Camera.Position, Camera.Position + Camera.Rotation.Forward * 250).Ignore(this).Run();

        if (useTrace.Hit) {
            switch (useTrace.Entity) {
                case PowerupEntity:
                case Goon: {
                    // hud write (E)
                    break;
                }
            }
        }

        if (!Input.Pressed(InputButton.Use)) return;

        switch (useTrace.Entity) {
            case Goon goon: {
                break;
            }
            case PowerupEntity powerupEnt: {
                if (Game.IsClient) {
                    if (!Hud._hud.RootPanel.ChildrenOfType<PowerupUI>().Any()) {
                        Hud._hud.RootPanel.AddChild(new PowerupUI(powerupEnt, this));
                    }
                }
                break;
            }
        } 
    }
}