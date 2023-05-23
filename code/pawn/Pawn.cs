using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GGame;

public partial class Pawn : AnimatedEntity {
    public Vector3 HeightOffset => new(0, 0,  35 * Scale);

    public WorldPanel healthPanel;
    public GoonStats stats;

    public Gun weapon;
    public TimeSince lastFire;
    public TimeSince reload;

    public List<Action<Pawn>> AttackActions = new();
    public List<Action<DamageInfo>> HurtActions = new();
    public List<Action<DamageInfo>> DieActions = new();

    [Net] public IList<AppliedPowerup> AppliedPowerups {get; set;} = new List<AppliedPowerup>();

    [Net] public bool IsInCombat {get; set;} = false;
    [Net] public int Team {get; set;} = 0;

    [Net] public float MaxHealth {get; set;} = 100;
    [Net] public new float Health {get; set;} = 100;

    [Net] public int Armor {get; set;} = 0;
    public float ArmorReduction => GetArmorReduction();

    [Net] public int BaseMoveSpeed {get; set;} = 200;
    [Net] public int AddMoveSpeed {get; set;} = 0;
    public int MoveSpeed => Math.Clamp(BaseMoveSpeed + AddMoveSpeed, 60, 1000);

    [Net] public float BaseWeaponDamage {get; set;} = 3;
    [Net] public float AddWeaponDamage {get; set;} = 0;
    public float WeaponDamage => Math.Max(BaseWeaponDamage + AddWeaponDamage, 0.2f);
    
    [Net] public float BaseFireRate {get; set;} = 0.3f;
    [Net] public float AddFireRate {get; set;} = 0;
    public float FireRate => Math.Clamp(BaseFireRate + AddFireRate, 0.05f, 1.5f);

    [Net] public float BaseReloadTime {get; set;} = 2;
    [Net] public float AddReloadTime {get; set;} = 0;
    public float ReloadTime => Math.Clamp(BaseReloadTime + AddReloadTime, 0.1f, 6);   

    [Net] public int BaseMagazineSize {get; set;} = 14;
    [Net] public int AddMagazineSize {get; set;} = 0;
    public int MagazineSize => Math.Max(BaseMagazineSize + AddMagazineSize, 2);

    [Net] public float BaseDegreeSpread {get; set;} = 3;
    [Net] public float AddDegreeSpread {get; set;} = 0;
    public float DegreeSpread => Math.Clamp(BaseDegreeSpread + AddDegreeSpread, 0, 8);

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

            TraceResult tr = Trace.Ray(Position + HeightOffset, target.Position + target.HeightOffset * 1.6f + spreadOffset)
                .WithoutTags($"team{Team}", "trigger")
                .Run();

            weapon.Fire(this, tr, WeaponDamage, () => {});
            CurrentMag -= 1;

            if (tr.Entity is Pawn goon) {
                foreach (Action<Pawn> act in AttackActions) {
                    act.Invoke(goon);
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

            Vector3 dir = Camera.Rotation.Forward + (new Vector3(0, spreadHoriz, spreadVert) * Camera.Rotation) * 0.015f;
            TraceResult tr = Trace.Ray(Camera.Position, Camera.Position + dir * (Range + 50))
                .WithoutTags($"team{Team}", "trigger")
                .Run();

            weapon.Fire(this, tr, WeaponDamage, () => {});
            CurrentMag -= 1;

            if (tr.Entity is Pawn goon && goon.Team != Team) {
                foreach (Action<Pawn> act in AttackActions) {
                    act.Invoke(goon);
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

        if (Team == 0) {
            GGame.Current.DamageTaken += newDamage;

            if (this == Player.Current) {
                Hud.TakeDamage();
                Player.FloatingText(Player.Current.Position + HeightOffset, info.Damage);
            }
        } else {
            GGame.Current.DamageDealt += newDamage;
        }   

        if (Health <= 0) {
            foreach (Action<DamageInfo> act in DieActions) {
                act.Invoke(info);
            }
            PlaySound("sounds/hurt.sound");
            OnKilled();
            return;
        }

        if (Random.Shared.Float(0, 1) > 0.7f) {
            PlaySound("sounds/hurt.sound");       
        }

        foreach (Action<DamageInfo> act in HurtActions) {
            act.Invoke(info);
        }
	}

    public float GetArmorReduction() {
        // ignore math if we have no armor or are hardcapped
        if (Armor < 1) return 1;
        if (Armor > 149) return 0.2f;

        // log, offset horizontally for goon flatter low range, and reduce vertical back to 0
        float logArmor = (float)Math.Log(Armor + 12) - 2.484f;
        // map so 150 armor reaches 0.8
        logArmor *= .308f;
        // invert, so 0 armor = 1, 150 armor = 0.2
        return 1 - logArmor;  
	}
    
    public DamageInfo QuickDamageInfo(float damage) {
        return new DamageInfo() {
            Damage = damage,
            Attacker = this,
            Weapon = weapon,
        };
    }
    public void PowerupLeech(Pawn pawn) {
        if (Random.Shared.Float(0, 1) < 0.166f) {
            Health = Math.Min(Health + 1, MaxHealth);
        }
    }
    public void PowerupCriticalHit(Pawn pawn) {
        if (Random.Shared.Float(0, 1) < 0.1f) {
            DamageInfo goon = QuickDamageInfo(WeaponDamage);
            pawn.TakeDamage(goon);
        }
    }

    public void PowerupThorns(DamageInfo info) {
        if (Random.Shared.Float(0, 1) < 0.333f) {
            info.Attacker.TakeDamage(QuickDamageInfo(info.Damage)); 
        }
    }

    public string[] PawnStrings() {
        return new string[] {
            $"favorite_border\n" +
            $"favorite\n" +
            $"shield\n" +
            $"directions_run\n" +
            $"rss_feed\n" +
            $"electric_bolt\n" +
            $"fast_forward\n" +
            $"expand\n" +
            $"all_out\n" +
            $"schedule"
        ,
            $"{MaxHealth:#0}\n" +
            $"{Health:#0}\n" +
            $"{Armor}\n" +
            $"{MoveSpeed}\n" +
            $"{Range:#0}\n" +
            $"{WeaponDamage}\n" +
            $"{FireRate:#0.00}\n" +
            $"{MagazineSize}\n" +
            $"{DegreeSpread:#0.0}\n" +
            $"{ReloadTime:#0.0}"
        ,
            $"\n" +
            $"\n" +
            $"\n" +
            $"({BaseMoveSpeed} + {AddMoveSpeed})\n" +
            $"({BaseRange:#0} + {AddRange:#0})\n" +
            $"({BaseWeaponDamage} + {AddWeaponDamage})\n" +
            $"({BaseFireRate:#0.00} + {AddFireRate:#0.00})\n" +
            $"({BaseMagazineSize} + {AddMagazineSize})\n" +
            $"({BaseDegreeSpread:#0.0} + {AddDegreeSpread:#0.0})\n" +
            $"({BaseReloadTime:#0.0} + {AddReloadTime:#0.0})"
        };
    }

    public string PawnStringSingle() {
        return $"MaxHP: {MaxHealth:#0}\n" +
            $"HP:    {Health:#0}\n" +
            $"Armor: {Armor}\n" +
            $"Speed: {MoveSpeed} ({BaseMoveSpeed} + {AddMoveSpeed})\n" +
            $"Range: {Range:#0} ({BaseRange:#0} + {AddRange:#0})\n" +
            $"Damage:{WeaponDamage} ({BaseWeaponDamage} + {AddWeaponDamage})\n" +
            $"Delay: {FireRate:#0.00} ({BaseFireRate:#0.00} + {AddFireRate:#0.00})\n" +
            $"Mag:   {MagazineSize} ({BaseMagazineSize} + {AddMagazineSize})\n" +
            $"Spread:{DegreeSpread:#0.0} ({BaseDegreeSpread:#0.0} + {AddDegreeSpread:#0.0})\n" +
            $"Reload:{ReloadTime:#0.0} ({BaseReloadTime:#0.0} + {AddReloadTime:#0.0})";
    }

    public string AmmoString() {
        return $"{CurrentMag}/{MagazineSize}";
    }
}