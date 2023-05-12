using Sandbox;
using System;

namespace GGame;

public class Gun : ModelEntity {
    public Vector3 muzzle;

    public void Init(string model) {
        SetModel(model);
        muzzle = Model.GetAttachment("muzzle")?.Position ?? Vector3.Zero;
    }
    public void Fire(Pawn owner, TraceResult tr, float damage, Action react) {
        DebugOverlay.Line(Position + muzzle * owner.Scale * owner.Rotation, tr.EndPosition, 0.1f, true);

        PlaySound("sounds/fire.sound");
        if (tr.Hit) {
            tr.Entity.TakeDamage(new DamageInfo() {
                Damage = damage,
                Attacker = owner,
                Weapon = this,
            });
            
            tr.Surface.DoBulletImpact(tr);

            if (owner == Player.Current && tr.Entity is Goon goon) {
                owner.PlaySound("sounds/hitsound.sound");

                Player.FloatingText(tr.EndPosition, damage * goon.ArmorReduction);
            }

            react.Invoke();
        }
    }
}