using Sandbox;
using System.Linq;

namespace GGame;

public partial class Player : Pawn {
    public Entity hitEnt;
    
    public void SimulateUse() {
        TraceResult useTrace = Trace.Ray(Camera.Position, Camera.Position + Camera.Rotation.Forward * 200).Ignore(this).Run();

        // for ui shit
        if (useTrace.Hit) {
            switch (useTrace.Entity) {
                case PowerupEntity: {
                    if (Game.IsClient) Hud._hud.epanel.AddClass("show");
                    break;
                }
                default: {
                    if (Game.IsClient) Hud._hud.epanel.RemoveClass("show");
                    break;
                }
            }
        } else {
            if (Game.IsClient) Hud._hud.epanel.RemoveClass("show");
        }

        if (!Input.Pressed(InputButton.Use)) return;

        // for using shit
        switch (useTrace.Entity) {
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