using Sandbox;
using System.Linq;

namespace GGame;

public partial class Player : Pawn {
    public Entity hitEnt;
    
    public void SimulateUse() {
        // for range
        TraceResult rangeTrace = Trace.Ray(Camera.Position, Camera.Position + Camera.Rotation.Forward * (Range + 50))
            .Ignore(this)
            .WithoutTags("team0", "trigger")
            .Run();
            
        if (Game.IsClient) {
            Hud.Current.crosshair.InRange(rangeTrace.Hit, DegreeSpread);
        }

        // for ui
        TraceResult useTrace = Trace.Ray(Camera.Position, Camera.Position + Camera.Rotation.Forward * 200)
            .Ignore(this)
            .WithoutTags("team0", "trigger")
            .Run();

        if (useTrace.Hit) {
            switch (useTrace.Entity) {
                case PowerupEntity: {
                    if (Game.IsClient) Hud.Current.epanel.AddClass("show");
                    break;
                }
                default: {
                    if (Game.IsClient) Hud.Current.epanel.RemoveClass("show");
                    break;
                }
            }
        } else {
            if (Game.IsClient) Hud.Current.epanel.RemoveClass("show");
        }

        if (!Input.Pressed(InputButton.Use)) return;

        // for using
        switch (useTrace.Entity) {
            case PowerupEntity powerupEnt: {
                if (Game.IsClient) {
                    if (!Hud.Current.RootPanel.ChildrenOfType<PowerupUI>().Any()) {
                        Hud.Current.RootPanel.AddChild(new PowerupUI(powerupEnt, this));
                    }
                }
                break;
            }
        } 
    }
}