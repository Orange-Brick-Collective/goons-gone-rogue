using Sandbox;
using System;
using System.Linq;

namespace GGame;

public enum Stat {
    Armor,
    Speed,
    Range,
    Damage,
    Delay,
    Mag,
    Spread,
    Reload
}

// this is purely for understanding and visualizing the powerup
public class SelectedStat {
    private static readonly Stat[] NegativeGoodStats = {Stat.Delay, Stat.Spread, Stat.Reload};

    public Stat stat;
    public float amount;
    public bool good;

    public SelectedStat() {}

    public SelectedStat(Stat stat, float amount) {
        this.stat = stat;
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
        new Powerup(
            "",
            "Heal Up",
            "Heals 40 health",
            null,
            (p) => p.Health = Math.Min(p.Health + 40, p.MaxHealth)
        ),
        new Powerup(
            "",
            "Heal Up",
            "Heals 60 health",
            null,
            (p) => p.Health = Math.Min(p.Health + 60, p.MaxHealth)
        ),
        new Powerup(
            "",
            "Big Heal Up",
            "Heals 100 health",
            null,
            (p) => p.Health = Math.Min(p.Health + 100, p.MaxHealth)
        ),
        new Powerup(
            "",
            "Hotter Bullets",
            "Adds 4 damage",
            new SelectedStat[] {new SelectedStat(Stat.Damage, 4)},
            (p) => p.AddWeaponDamage += 4
        ),
        new Powerup(
            "",
            "Trigger Happy",
            "Fires 0.02 seconds faster",
            new SelectedStat[] {new SelectedStat(Stat.Delay, -0.02f)},
            (p) => p.AddFireRate -= 0.02f
        ),
        new Powerup(
            "",
            "Sharper Eyes",
            "Adds 100 range",
            new SelectedStat[] {new SelectedStat(Stat.Range, 100)},
            (p) => p.AddRange += 100
        ),
        new Powerup(
            "",
            "Extra Padding",
            "Adds 60 max health",
            null,
            (p) => p.MaxHealth += 60
        ),
        new Powerup(
            "",
            "Extra Padding",
            "Adds 80 max health",
            null,
            (p) => p.MaxHealth += 80
        ),
        new Powerup(
            "",
            "Accurate",
            "Adds 0.3 less spread",
            new SelectedStat[] {new SelectedStat(Stat.Spread, -0.3f)},
            (p) => p.AddDegreeSpread -= 0.3f
        ),
        new Powerup(
            "",
            "Longer Mag",
            "Adds 6 more bullets to magazine",
            new SelectedStat[] {new SelectedStat(Stat.Mag, 6)},
            (p) => p.AddMagazineSize += 6
        ),
        new Powerup(
            "",
            "Quick Hands",
            "Speeds up reload by 0.4s",
            new SelectedStat[] {new SelectedStat(Stat.Reload, -0.4f)},
            (p) => p.AddReloadTime -= 0.4f
        ),
        new Powerup(
            "",
            "Sprinter",
            "Moves 80 faster",
            new SelectedStat[] {new SelectedStat(Stat.Speed, 80)},
            (p) => p.AddMoveSpeed += 80
        ),
        new Powerup(
            "",
            "Thicker Armor",
            "Adds 4 armor",
            new SelectedStat[] {new SelectedStat(Stat.Armor, 4)},
            (p) => p.Armor += 4
        ),

        // *
        // * Multi step actions
        // *
        new Powerup(
            "",
            "Glass Cannon",
            "Adds 16 damage, but sets health and max health to 25",
            new SelectedStat[] {new SelectedStat(Stat.Damage, 16)},
            PowerupGlassCannon
        ),
        new Powerup(
            "",
            "Speedy Cheesy",
            "Fire 0.04 seconds faster and moves 80 speed faster, but loses 100 range and 0.6 spread",
            new SelectedStat[] {
                new SelectedStat(Stat.Delay, -0.04f), 
                new SelectedStat(Stat.Speed, 80),
                new SelectedStat(Stat.Range, -100),
                new SelectedStat(Stat.Spread, 0.6f),
            },
            PowerupSpeedyCheesy
        ),
        new Powerup(
            "",
            "Tank",
            "Gains 25 armor, but loses 100 range and 150 move speed",
            new SelectedStat[] {
                new SelectedStat(Stat.Armor, 25),
                new SelectedStat(Stat.Range, -100),
                new SelectedStat(Stat.Speed, -150),
            },
            PowerupTank
        ),
        new Powerup(
            "",
            "Sniper Rounds",
            "Add 10 damage and 100 range, but lose 0.14 firerate and 15 mag",
            new SelectedStat[] {
                new SelectedStat(Stat.Damage, 10),
                new SelectedStat(Stat.Range, 100),
                new SelectedStat(Stat.Delay, 0.14f),
                new SelectedStat(Stat.Mag, -15),
            },
            PowerupSniperRounds
        ),
        new Powerup(
            "",
            "Trigger Happy",
            "Fire 0.06 faster, but lose 0.8 spread",
            new SelectedStat[] {
                new SelectedStat(Stat.Delay, -0.06f), 
                new SelectedStat(Stat.Spread, 0.8f),
            },
            PowerupTriggerHappy
        ),
        // *
        // * Attack Actions
        // *
        new Powerup(
            "",
            "Leech",
            "1/6 chance every attack to regen one health. Every additional 'Leech' is another 1/6 chance for another one health",
            null,
            (p) => p.AttackActions.Add(p.PowerupLeech)
        ),
        new Powerup(
            "",
            "Critical Hit",
            "1/10 chance every attack to deal an additional hit. Every additional 'Critical Hit' is another 1/10 chance for another hit",
            null,
            (p) => p.AttackActions.Add(p.PowerupCriticalHit)
        ),


        // *
        // * Hurt Actions
        // *
        new Powerup(
            "",
            "Thorns",
            "1/3 chance when hurt to deal damage back to attacker. Every additional 'Thorns' is another 1/3 chance to return damage",
            null,
            (p) => p.HurtActions.Add(p.PowerupThorns)
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
    public virtual string Image {get; set;} = "unset";
    public virtual string Title {get; set;} = "unset";
    public virtual string Description {get; set;} = "unset";
    public virtual Action<Pawn> Action {get; set;} = (p) => {};
    public SelectedStat[] AffectedStats {get; set;}

    public Powerup() {}
    public Powerup(string image, string title, string desc, SelectedStat[] stat, Action<Pawn> action) {
        Image = image;
        Title = title;
        Description = desc;
        Action = action;
        AffectedStats = stat;
    }

    // kept having problems of Action going null, just a sanity check
    public Powerup(Powerup copy) {
        Image = copy.Image;
        Title = copy.Title;
        Description = copy.Description;
        Action = copy.Action;
    }
}

    // new Powerup(
    //     "",
    //     "Leech",
    //     "1/5 chance every attack to regen one health. Every additional leech is another 1/5 chance for another one health",
    //     (p) => p.AttackActions.Add(() => {
    //         if (Random.Shared.Float(0, 1) > 0.8f) {
    //             p.Health = Math.Min(p.Health + 1, p.MaxHealth);
    //         }
    //     })
    // ),
    
    // new Powerup(
    //     "",
    //     "Glass Cannon",
    //     "Sets health to 50 but doubles their non-base damage",
    //     (p) => {
    //         p.MaxHealth = 50;
    //         p.Health = 50;
    //         p.AddWeaponDamage = p.WeaponDamage * 2;
    //     }
    // ),