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
    public void Fire(Pawn owner, TraceResult tr, int damage, Action react) {
        DebugOverlay.Line(Position + muzzle * owner.Scale * owner.Rotation, tr.EndPosition, 0.1f, true);

        PlaySound("sounds/fire.sound");
        if (tr.Hit) {
            tr.Entity.TakeDamage(new DamageInfo() {
                Damage = damage,
                Attacker = owner,
                Weapon = this,
            });
            
            tr.Surface.DoBulletImpact(tr);

            if (owner == Player.Cur && tr.Entity is Goon) {
                owner.PlaySound("sounds/hitsound.sound");
            }

            react.Invoke();
        }
    }
}