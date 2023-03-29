using Sandbox;
using System;
using System.Collections.Generic;

namespace GGame;

public partial class Pawn : AnimatedEntity {
    public Vector3 HeightOffset => new(0, 0,  35 * Scale);

    public Gun weapon;
    public TimeSince lastFire;

    public List<Action> AttackActions = new();
    public List<Action> HurtActions = new();

    [Net] public bool IsInCombat {get; set;} = true;
    [Net] public int Team {get; set;} = 0;

    [Net] public float MaxHealth {get; set;} = 100;
    [Net] public new float Health {get; set;} = 100;

    [Net] public int Armor {get; set;} = 0;
    public float ArmorReduction => GetArmorReduction();

    [Net] public int BaseMoveSpeed {get; set;} = 200;
    [Net] public int AddMoveSpeed {get; set;} = 0;
    public int MoveSpeed => BaseMoveSpeed + AddMoveSpeed;

    [Net] public float BaseFireRate {get; set;} = 0.33f;
    [Net] public float AddFireRate {get; set;} = 0;
    public float FireRate => Math.Max(BaseFireRate + AddFireRate, 0.05f);

    [Net] public int BaseWeaponDamage {get; set;} = 10;
    [Net] public int AddWeaponDamage {get; set;} = 0;
    public int WeaponDamage => BaseWeaponDamage + AddWeaponDamage;

    [Net] public float BaseDegreeSpread {get; set;} = 5;
    [Net] public float AddDegreeSpread {get; set;} = -3;
    public float DegreeSpread => BaseDegreeSpread + AddDegreeSpread;

    [Net] public float BaseRange {get; set;} = 400;
    [Net] public float AddRange {get; set;} = 0;
    public float Range => BaseRange + AddRange;

    public void OnEnemyKilled() {
		// check boosts
	}

    public void FireGun(Pawn target) {
        if (weapon is null) return;

        TraceResult tr = Trace.Ray(Position + HeightOffset, target.Position + target.HeightOffset)
            .WithoutTags($"team{Team}")
            .Run();

        if (lastFire > FireRate) {
            lastFire = 0;
            weapon.Fire(this, tr, WeaponDamage, () => {});

            foreach (Action act in AttackActions) {
                act.Invoke();
            }
        } 
    }

    public override void TakeDamage(DamageInfo info) {
        float newDamage = info.Damage * ArmorReduction;
        Health -= newDamage;

        if (Health <= 0) OnKilled();

        foreach (Action act in HurtActions) {
            act.Invoke();
        }
	}

    public float GetArmorReduction() {
        // ignore math if we have no armor or are hardcapped
        if (Armor == 0) return 1;
        if (Armor > 149) return 0.1f;

        // log, offset horizontally for a smoother increase, and reset vertical back to 0
        float logArmor = (float)Math.Log(Armor + 12) - 2.484f;
        // map so 150 armor reaches 0.9
        logArmor *= .343f;
        // invert, so 0 armor = 1, 150 armor = 0.1
        return 1 - logArmor;  
	}

    public void PowerupLeech() {
        Log.Info("leech");
        if (Random.Shared.Float(0, 1) > 0.8f) {
            Health = Math.Min(Health + 1, MaxHealth);
        }
    }
}