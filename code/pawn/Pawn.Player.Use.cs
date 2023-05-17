using Sandbox;
using System.Linq;

namespace GGame;

public partial class Player : Pawn {
    public Entity hitEnt;
    
    public void SimulateUse() {
        // for range
        TraceResult rangeTrace = Trace.Ray(Camera.Position, Camera.Position + Camera.Rotation.Forward * (Range + 45))
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

        if (useTrace.Hit && useTrace.Entity is IUse eg) {
            if (Game.IsClient) Hud.Current.usePopupPanel.AddClass("show");
        } else {
            if (Game.IsClient) Hud.Current.usePopupPanel.RemoveClass("show");
            return;
        }

        if (Input.Pressed("use")) eg.OnUse(this);     
    }
}