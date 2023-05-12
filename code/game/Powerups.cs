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
    public static int GetRandomIndex => Random.Shared.Int(0, list.Length - 1);

    public static Powerup GetByIndex(int index) {
        return list[Math.Clamp(index, 0, list.Length - 1)];
    }

    // ! actions with ';' break even
    public static readonly Powerup[] list = new Powerup[] {
        // *
        // * simple actions
        // *
        new PowerupStat(
            "/images/icons/plus.png",
            "Heal Up",
            "Delicious milkshake",
            new SelectedStat[] {new SelectedStat(Stat.Health, 50)}
        ),
        new PowerupStat(
            "/images/icons/plus.png",
            "Big Heal Up",
            "Big meat dinner brings you back",
            new SelectedStat[] {new SelectedStat(Stat.Health, 80)}
        ),
        new PowerupStat(
            "/images/icons/bullet.png",
            "Hotter Bullets",
            "Got these bullets that have extra powder",
            new SelectedStat[] {new SelectedStat(Stat.AddWeaponDamage, 5)}
        ),
        new PowerupStat(
            "/images/icons/trigger.png",
            "Trigger Happy",
            "Training to pull the trigger quicker",
            new SelectedStat[] {new SelectedStat(Stat.AddFireRate, -0.03f)}
        ),
        new PowerupStat(
            "/images/icons/eye.png",
            "Sharper Eyes",
            "Glasses really help you see",
            new SelectedStat[] {new SelectedStat(Stat.AddRange, 120)}
        ),
        new PowerupStat(
            "/images/icons/plus_up.png",
            "Extra Padding",
            "A bit of working out helps a lot",
            new SelectedStat[] {new SelectedStat(Stat.MaxHealth, 60)}
        ),
        new PowerupStat(
            "/images/icons/plus_up.png",
            "Extra Padding",
            "Taking one of these pills make you feel better than ever",
            new SelectedStat[] {new SelectedStat(Stat.MaxHealth, 100)}
        ),
        new PowerupStat(
            "/images/icons/target.png",
            "Accurate",
            "Who knew holding your hands still helped",
            new SelectedStat[] {new SelectedStat(Stat.AddDegreeSpread, -0.4f)}
        ),
        new PowerupStat(
            "/images/icons/mag.png",
            "Longer Mag",
            "Snached this even larger drum mag!",
            new SelectedStat[] {new SelectedStat(Stat.AddMagazineSize, 8)}
        ),
        new PowerupStat(
            "/images/icons/hand.png",
            "Quick Hands",
            "Tactical reloads really work!",
            new SelectedStat[] {new SelectedStat(Stat.AddReloadTime, -0.4f)}
        ),
        new PowerupStat(
            "/images/icons/",
            "Sprinter",
            "Could be training, could be gamer shoes",
            new SelectedStat[] {new SelectedStat(Stat.AddMoveSpeed, 100)}
        ),
        new PowerupStat(
            "/images/icons/",
            "Thicker Armor",
            "An extra plate can't hurt",
            new SelectedStat[] {new SelectedStat(Stat.Armor, 6)}
        ),

        // *
        // * combo simple actions
        // *
        new PowerupStat(
            "/images/icons/glass.png",
            "Glass Cannon",
            "Hit strong, get hit strong",
            new SelectedStat[] {
                new SelectedStat(Stat.AddWeaponDamage, 16),
                new SelectedStat(Stat.Health, 25, Op.Set),
                new SelectedStat(Stat.MaxHealth, 25, Op.Set),
            }
        ),
        new PowerupStat(
            "/images/icons/",
            "Speedy Cheesy",
            "With great speed comes imprecision",
            new SelectedStat[] {
                new SelectedStat(Stat.AddFireRate, -0.04f), 
                new SelectedStat(Stat.AddMoveSpeed, 80),
                new SelectedStat(Stat.AddDegreeSpread, 0.6f),
            }
        ),
        new PowerupStat(
            "/images/icons/",
            "Tank",
            "Slow and steady",
            new SelectedStat[] {
                new SelectedStat(Stat.Armor, 25),
                new SelectedStat(Stat.AddRange, -100),
                new SelectedStat(Stat.AddMoveSpeed, -100),
            }
        ),
        new PowerupStat(
            "/images/icons/",
            "Sniper Rounds",
            "These bigger rounds pack a punch, but you cant use as many",
            new SelectedStat[] {
                new SelectedStat(Stat.AddWeaponDamage, 10),
                new SelectedStat(Stat.AddRange, 100),
                new SelectedStat(Stat.AddFireRate, 0.14f),
                new SelectedStat(Stat.AddMagazineSize, -8),
            }
        ),
        new PowerupStat(
            "/images/icons/",
            "Trigger Happy",
            "Spray all day",
            new SelectedStat[] {
                new SelectedStat(Stat.AddFireRate, -0.06f), 
                new SelectedStat(Stat.AddDegreeSpread, 0.8f),
            }
        ),

        // *
        // * attack actions
        // *
        new PowerupPawnAct(
            "/images/icons/",
            "Leech",
            "1/6 chance every attack to regen one health. Every additional 'Leech' is another 1/6 chance for another one health",
            (pawn) => pawn.AttackActions.Add(pawn.PowerupLeech)
        ),
        new PowerupPawnAct(
            "/images/icons/",
            "Critical Hit",
            "1/10 chance every attack to deal an additional hit. Every additional 'Critical Hit' is another 1/10 chance for another hit",
            (pawn) => pawn.AttackActions.Add(pawn.PowerupCriticalHit)
        ),


        // *
        // * hurt actions
        // *
        new PowerupPawnAct(
            "/images/icons/",
            "Thorns",
            "1/3 chance when hurt to deal damage back to attacker. Every additional 'Thorns' is another 1/3 chance to return damage",
            (pawn) => pawn.HurtActions.Add(pawn.PowerupThorns)
        ),

        // *
        // * something Actions
        // *
    };
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