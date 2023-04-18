using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GGame;

public enum Stat {
    MaxHealth,
    Health,
    Armor,
    AddMoveSpeed,
    AddRange,
    AddWeaponDamage,
    AddFireRate,
    AddMagazineSize,
    AddDegreeSpread,
    AddReloadTime,
}

public enum Op {
    Add,
    Mult,
    Set
}

// this is purely for understanding and visualizing the powerup
public class SelectedStat {
    private static readonly Stat[] NegativeGoodStats = {Stat.AddFireRate, Stat.AddDegreeSpread, Stat.AddReloadTime};

    public Stat stat;
    public Op op;
    public float amount;
    public bool good;

    public SelectedStat() {}

    public SelectedStat(Stat stat, float amount, Op op = Op.Add) {
        this.stat = stat;
        this.op = op;
        this.amount = amount;

        if (NegativeGoodStats.Contains(stat)) {
            if (amount < 0) good = true;
            else good = false;
        } else {
            if (amount > 0) good = true;
            else good = false;
        }
    }
}

public class Powerups {
    public static Powerup GetRandom => list[Random.Shared.Int(0, list.Length - 1)];
    public static int GetRandomIndex => Random.Shared.Int(0, list.Length - 1);

    public static Powerup GetByIndex(int index) {
        return list[Math.Clamp(index, 0, list.Length - 1)];
    }

    // ! actions with ';' break even
    public static readonly Powerup[] list = new Powerup[] {
        // *
        // * Simple actions
        // *
        new PowerupStat(
            "",
            "Heal Up",
            "Heals 50 health",
            new SelectedStat[] {new SelectedStat(Stat.Health, 50)}
        ),
        new PowerupStat(
            "",
            "Heal Up",
            "Heals 75 health",
            new SelectedStat[] {new SelectedStat(Stat.Health, 75)}
        ),
        new PowerupStat(
            "",
            "Big Heal Up",
            "Heals 120 health",
            new SelectedStat[] {new SelectedStat(Stat.Health, 120)}
        ),
        new PowerupStat(
            "",
            "Hotter Bullets",
            "Adds 5 damage",
            new SelectedStat[] {new SelectedStat(Stat.AddWeaponDamage, 5)}
        ),
        new PowerupStat(
            "",
            "Trigger Happy",
            "Fires 0.03 seconds faster",
            new SelectedStat[] {new SelectedStat(Stat.AddFireRate, -0.03f)}
        ),
        new PowerupStat(
            "",
            "Sharper Eyes",
            "Adds 120 range",
            new SelectedStat[] {new SelectedStat(Stat.AddRange, 120)}
        ),
        new PowerupStat(
            "",
            "Extra Padding",
            "Adds 80 max health",
            new SelectedStat[] {new SelectedStat(Stat.MaxHealth, 80)}
        ),
        new PowerupStat(
            "",
            "Extra Padding",
            "Adds 120 max health",
            new SelectedStat[] {new SelectedStat(Stat.MaxHealth, 120)}
        ),
        new PowerupStat(
            "",
            "Accurate",
            "Adds 0.4 less spread",
            new SelectedStat[] {new SelectedStat(Stat.AddDegreeSpread, -0.4f)}
        ),
        new PowerupStat(
            "",
            "Longer Mag",
            "Adds 8 more bullets to magazine",
            new SelectedStat[] {new SelectedStat(Stat.AddMagazineSize, 8)}
        ),
        new PowerupStat(
            "",
            "Quick Hands",
            "Speeds up reload by 0.4s",
            new SelectedStat[] {new SelectedStat(Stat.AddReloadTime, -0.4f)}
        ),
        new PowerupStat(
            "",
            "Sprinter",
            "Moves 100 faster",
            new SelectedStat[] {new SelectedStat(Stat.AddMoveSpeed, 100)}
        ),
        new PowerupStat(
            "",
            "Thicker Armor",
            "Adds 6 armor",
            new SelectedStat[] {new SelectedStat(Stat.Armor, 6)}
        ),

        new PowerupStat(
            "",
            "Glass Cannon",
            "Adds 16 damage, but sets health and max health to 25",
            new SelectedStat[] {
                new SelectedStat(Stat.AddWeaponDamage, 16),
                new SelectedStat(Stat.Health, 25, Op.Set),
                new SelectedStat(Stat.MaxHealth, 25, Op.Set),
            }
        ),
        new PowerupStat(
            "",
            "Speedy Cheesy",
            "Fire 0.04 seconds faster and moves 80 speed faster, but loses 100 range and 0.6 spread",
            new SelectedStat[] {
                new SelectedStat(Stat.AddFireRate, -0.04f), 
                new SelectedStat(Stat.AddMoveSpeed, 80),
                new SelectedStat(Stat.AddRange, -100),
                new SelectedStat(Stat.AddDegreeSpread, 0.6f),
            }
        ),
        new PowerupStat(
            "",
            "Tank",
            "Gains 25 armor, but loses 100 range and 150 move speed",
            new SelectedStat[] {
                new SelectedStat(Stat.Armor, 25),
                new SelectedStat(Stat.AddRange, -100),
                new SelectedStat(Stat.AddMoveSpeed, -150),
            }
        ),
        new PowerupStat(
            "",
            "Sniper Rounds",
            "Add 10 damage and 100 range, but lose 0.14 firerate and 15 mag",
            new SelectedStat[] {
                new SelectedStat(Stat.AddWeaponDamage, 10),
                new SelectedStat(Stat.AddRange, 100),
                new SelectedStat(Stat.AddFireRate, 0.14f),
                new SelectedStat(Stat.AddMagazineSize, -15),
            }
        ),
        new PowerupStat(
            "",
            "Trigger Happy",
            "Fire 0.06 faster, but lose 0.8 spread",
            new SelectedStat[] {
                new SelectedStat(Stat.AddFireRate, -0.06f), 
                new SelectedStat(Stat.AddDegreeSpread, 0.8f),
            }
        ),

        // *
        // * Attack Actions
        // *
        new PowerupPawnAct(
            "",
            "Leech",
            "1/6 chance every attack to regen one health. Every additional 'Leech' is another 1/6 chance for another one health",
            (pawn) => pawn.AttackActions.Add(pawn.PowerupLeech)
        ),
        new PowerupPawnAct(
            "",
            "Critical Hit",
            "1/10 chance every attack to deal an additional hit. Every additional 'Critical Hit' is another 1/10 chance for another hit",
            (pawn) => pawn.AttackActions.Add(pawn.PowerupCriticalHit)
        ),


        // *
        // * Hurt Actions
        // *
        new PowerupPawnAct(
            "",
            "Thorns",
            "1/3 chance when hurt to deal damage back to attacker. Every additional 'Thorns' is another 1/3 chance to return damage",
            (pawn) => pawn.HurtActions.Add(pawn.PowerupThorns)
        ),

        // *
        // * Attack Actions
        // *
    };

    public static void PowerupGlassCannon(Pawn pawn) {
        pawn.MaxHealth = 25;
        pawn.Health = 25;
        pawn.AddWeaponDamage += 16;
    }
    public static void PowerupSpeedyCheesy(Pawn pawn) {
        pawn.AddFireRate -= 0.04f;
        pawn.AddMoveSpeed += 80;
        pawn.AddRange -= 100;
        pawn.AddDegreeSpread += 0.6f;
    }
    public static void PowerupTank(Pawn pawn) {
        pawn.Armor += 25;
        pawn.AddMoveSpeed -= 100;
        pawn.AddRange -= 100;
    } 
    public static void PowerupSniperRounds(Pawn pawn) {
        pawn.AddWeaponDamage += 10;
        pawn.AddRange += 100;
        pawn.AddFireRate += 0.14f;
        pawn.AddMagazineSize -= 15;
    }
    public static void PowerupTriggerHappy(Pawn pawn) {
        pawn.AddFireRate -= 0.06f;
        pawn.AddDegreeSpread += 0.8f;
    }
}

public class Powerup {
    public virtual string Image {get; set;}
    public virtual string Title {get; set;}
    public virtual string Description {get; set;}

    public Powerup() {}
}

public class PowerupStat : Powerup {
    public override string Image {get; set;} = "unset";
    public override string Title {get; set;} = "unset";
    public override string Description {get; set;} = "unset";
    public SelectedStat[] AffectedStats {get; set;}

    public PowerupStat() {}
    public PowerupStat(string image, string title, string desc, SelectedStat[] stat) {
        Image = image;
        Title = title;
        Description = desc;
        AffectedStats = stat;
    }
}

public class PowerupPawnAct : Powerup {
    public override string Image {get; set;} = "unset";
    public override string Title {get; set;} = "unset";
    public override string Description {get; set;} = "unset";
    public Action<Pawn> Action {get; set;}

    public PowerupPawnAct() {}
    public PowerupPawnAct(string image, string title, string desc, Action<Pawn> action) {
        Image = image;
        Title = title;
        Description = desc;
        Action = action;
    }
}

// public class PowerupHurtAct : Powerup {
//     public override string Image {get; set;} = "unset";
//     public override string Title {get; set;} = "unset";
//     public override string Description {get; set;} = "unset";
//     public Action<DamageInfo> Action {get; set;}

//     public PowerupHurtAct() {}
//     public PowerupHurtAct(string image, string title, string desc, Action<DamageInfo> action) {
//         Image = image;
//         Title = title;
//         Description = desc;
//         Action = action;
//     }
// }