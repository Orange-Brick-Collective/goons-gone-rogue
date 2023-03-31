using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;

namespace GGame;

public partial class Pawn : AnimatedEntity {
    public Vector3 HeightOffset => new(0, 0,  35 * Scale);

    public WorldPanel healthPanel;

    public Gun weapon;
    public TimeSince lastFire;
    public TimeSince reload;

    public List<Action<Pawn>> AttackActions = new();
    public List<Action<DamageInfo>> HurtActions = new();
    public List<Action<DamageInfo>> DieActions = new();

    // ! make work
    // gets added and removed actively (not functional)
    public List<Action> TickActions = new();

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
    public int WeaponDamage => Math.Max(BaseWeaponDamage + AddWeaponDamage, 1);

    [Net] public float BaseReloadTime {get; set;} = 2;
    [Net] public float AddReloadTime {get; set;} = 0;
    public float ReloadTime => Math.Max(BaseReloadTime + AddReloadTime, 0.1f);   

    [Net] public int BaseMagazineSize {get; set;} = 20;
    [Net] public int AddMagazineSize {get; set;} = 0;
    public int MagazineSize => Math.Max(BaseMagazineSize + AddMagazineSize, 2);

    [Net] public float BaseDegreeSpread {get; set;} = 2;
    [Net] public float AddDegreeSpread {get; set;} = 0;
    public float DegreeSpread => Math.Max(BaseDegreeSpread + AddDegreeSpread, 0);

    [Net] public float BaseRange {get; set;} = 400;
    [Net] public float AddRange {get; set;} = 0;
    public float Range => Math.Max(BaseRange + AddRange, 100);

    [Net] public int CurrentMag {get; set;} = 20;

    public void FireGun(Pawn target) {
        if (weapon is null) return;

        if (CurrentMag < 1) {
            if (reload < ReloadTime) return;
            CurrentMag = MagazineSize;
        }

        if (lastFire > FireRate) {
            lastFire = 0;

            float spreadVert = Random.Shared.Float(-DegreeSpread, DegreeSpread);
            float spreadHoriz = Random.Shared.Float(-DegreeSpread, DegreeSpread);
            Vector3 spreadOffset = new Vector3(spreadHoriz, spreadHoriz, spreadVert) * 5;

            TraceResult tr = Trace.Ray(Position + HeightOffset, target.Position + target.HeightOffset + spreadOffset)
                .WithoutTags($"team{Team}", "trigger")
                .Run();

            weapon.Fire(this, tr, WeaponDamage, () => {});
            CurrentMag -= 1;

            if (tr.Entity is Pawn a && a.Team != Team) {
                foreach (Action<Pawn> act in AttackActions) {
                    act.Invoke(a);
                }
            }

            if (CurrentMag < 1) {
                reload = 0;
            }
        } 
    }
    public void FireGun() {
        if (weapon is null) return;

        if (CurrentMag < 1) {
            if (reload < ReloadTime) return;
            CurrentMag = MagazineSize;
        }

        if (lastFire > FireRate) {
            lastFire = 0;

            float spreadVert = Random.Shared.Float(-DegreeSpread, DegreeSpread);
            float spreadHoriz = Random.Shared.Float(-DegreeSpread, DegreeSpread);

            Vector3 dir = Camera.Rotation.Forward + new Vector3(spreadHoriz, spreadHoriz, spreadVert) * 0.015f;
            TraceResult tr = Trace.Ray(Camera.Position, Camera.Position + dir * 2000)
                .WithoutTags($"team{Team}", "trigger")
                .Run();

            weapon.Fire(this, tr, WeaponDamage, () => {});
            CurrentMag -= 1;

            if (tr.Entity is Pawn a && a.Team != Team) {
                foreach (Action<Pawn> act in AttackActions) {
                    act.Invoke(a);
                }
            }

            if (CurrentMag < 1) {
                reload = 0;
            }
        } 
    }

    public override void TakeDamage(DamageInfo info) {
        float newDamage = info.Damage * ArmorReduction;
        Health -= newDamage;

        if (Health <= 0) {
            foreach (Action<DamageInfo> act in DieActions) {
                act.Invoke(info);
            }
            OnKilled();
            return;
        }

        foreach (Action<DamageInfo> act in HurtActions) {
            act.Invoke(info);
        }
	}

    public float GetArmorReduction() {
        // ignore math if we have no armor or are hardcapped
        if (Armor == 0) return 1;
        if (Armor > 149) return 0.1f;

        // log, offset horizontally for a flatter low range, and reduce vertical back to 0
        float logArmor = (float)Math.Log(Armor + 12) - 2.484f;
        // map so 150 armor reaches 0.9
        logArmor *= .343f;
        // invert, so 0 armor = 1, 150 armor = 0.1
        return 1 - logArmor;  
	}

    public DamageInfo QuickDamageInfo(float damage) {
        return new DamageInfo() {
            Damage = damage,
            Attacker = this,
            Weapon = weapon,
        };
    }

    public string PawnString() {
        return $@"{Name}
        Speed: {BaseMoveSpeed} + {AddMoveSpeed}
        Damage: {BaseWeaponDamage} + {AddWeaponDamage}
        RPM: {BaseFireRate} + {AddFireRate}
        Mag: {BaseMagazineSize} + {AddMagazineSize}
        Spread: {BaseDegreeSpread} + {AddDegreeSpread}
        Reload: {BaseReloadTime} + {AddReloadTime}
        Range: {BaseRange} + {AddRange}
        Armor: {Armor}
        ";
    }

    public string AmmoString() {
        return $"{CurrentMag} / {MagazineSize}";
    }

    public void PowerupGlassCannon() {
        MaxHealth = 50;
        Health = 50;
        AddWeaponDamage *= 2;
    }
    public void PowerupSpeedyCheesy() {
        AddFireRate -= 0.08f;
        AddMoveSpeed += 150;
        AddRange -= 100;
    }

    public void PowerupLeech(Pawn pawn) {
        if (Random.Shared.Float(0, 1) < 0.166f) {
            Health = Math.Min(Health + 1, MaxHealth);
        }
    }
    public void PowerupCriticalHit(Pawn pawn) {
        if (Random.Shared.Float(0, 1) < 0.1f) {
            pawn.TakeDamage(QuickDamageInfo(WeaponDamage));
        }
    }

    public void PowerupThorns(DamageInfo info) {
        if (Random.Shared.Float(0, 1) < 0.333f) {
            info.Attacker.TakeDamage(QuickDamageInfo(info.Damage * 0.5f));
        }
    }
}