using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GGame;

public class Gun : ModelEntity {
    public Vector3 muzzle;

    public void Init(string model) {
        SetModel(model);
        muzzle = Model.GetAttachment("muzzle")?.Position ?? Vector3.Zero;
    }
    public void Fire(AnimatedEntity owner, int damage, Action react) {
        Trace t = Trace.Ray(Position + (muzzle * owner.Rotation), Position + owner.Rotation.Forward * 1500);
        TraceResult tr = t.Ignore(owner).Run();

        DebugOverlay.Line(Position + (muzzle * owner.Rotation), tr.EndPosition, 0.1f, true);

        PlaySound("sounds/fire.sound");
        if (tr.Hit) {
            tr.Entity.TakeDamage(new DamageInfo() {
                Damage = damage, 
                Attacker = owner,
                Weapon = this,
            });
            react.Invoke();
        }
    }
}