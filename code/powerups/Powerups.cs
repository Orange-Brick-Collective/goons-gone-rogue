using Sandbox;
using System;

namespace GGame;

public class Powerups {
    public static Powerup GetRandom => list[Random.Shared.Int(0, list.Length - 1)];
    public static int GetRandomIndex => Random.Shared.Int(0, list.Length - 1);

    public static Powerup GetByIndex(int index) {
        return list[Math.Clamp(index, 0, list.Length - 1)];
    }

    // ! all actions currently null on hotload
    // ! and actions with ';' break even more
    public static readonly Powerup[] list = new Powerup[] {
        // *
        // * Simple actions
        // *
        new Powerup(
            "",
            "Hotter Bullets",
            "Adds 2 damage",
            (p) => p.AddWeaponDamage += 2
        ),
        new Powerup(
            "",
            "Trigger Happy",
            "Fires 0.05 seconds faster",
            (p) => p.AddFireRate -= 0.05f
        ),
        new Powerup(
            "",
            "Sharper Eyes",
            "Adds 50 range",
            (p) => p.AddRange += 50
        ),
        new Powerup(
            "",
            "Extra Padding",
            "Adds 50 max health",
            (p) => p.MaxHealth += 50
        ),
        new Powerup(
            "",
            "Heal Up",
            "Heals 20 health",
            (p) => p.Health = Math.Min(p.Health + 20, p.MaxHealth)
        ),
        new Powerup(
            "",
            "Big Heal Up",
            "Heals 40 health",
            (p) => p.Health = Math.Min(p.Health + 40, p.MaxHealth)
        ),
        new Powerup(
            "",
            "Longer Mag",
            "Adds 2 more bullets to magazine",
            (p) => p.AddMagazineSize += 2
        ),
        new Powerup(
            "",
            "Quick Hands",
            "Speeds up reload by 0.2s",
            (p) => p.AddReloadTime -= 0.2f
        ),
        new Powerup(
            "",
            "Thicker Armor",
            "Adds 1 armor",
            (p) => p.Armor++
        ),

        // *
        // * Multi step actions
        // *
        new Powerup(
            "",
            "Glass Cannon",
            "Sets health to 50 but doubles their non-base damage",
            (p) => p.PowerupGlassCannon()
        ),
        new Powerup(
            "",
            "Speedy Cheesy",
            "Fire 0.08 seconds faster and moves 150 speed faster, but loses 100 range",
            (p) => p.PowerupSpeedyCheesy()
        ),
        new Powerup(
            "",
            "Tank",
            "Gains 25 armor, but loses 100 range and move speed",
            (p) => p.PowerupTank()
        ),
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

        // *
        // * Attack Actions
        // *
        new Powerup(
            "",
            "Leech",
            "1/6 chance every attack to regen one health. Every additional 'Leech' is another 1/6 chance for another one health",
            (p) => p.AttackActions.Add(p.PowerupLeech)
        ),
        new Powerup(
            "",
            "Critical Hit",
            "1/10 chance every attack to deal an additional hit. Every additional 'Critical Hit' is another 1/10 chance for another hit",
            (p) => p.AttackActions.Add(p.PowerupCriticalHit)
        ),
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

        // *
        // * Hurt Actions
        // *
        new Powerup(
            "",
            "Thorns",
            "1/3 chance when hurt to deal half damage back to attacker. Every additional 'Thorns' is another 1/3 chance to return damage",
            (p) => p.HurtActions.Add(p.PowerupThorns)
        ),

        // *
        // * Attack Actions
        // *
    };
}

public class Powerup {
    public virtual string Image {get; set;} = "unset";
    public virtual string Title {get; set;} = "unset";
    public virtual string Description {get; set;} = "unset";
    public virtual Action<Pawn> Action {get; set;} = (p) => {};

    public Powerup() {}
    public Powerup(string image, string title, string desc, Action<Pawn> action) {
        Image = image;
        Title = title;
        Description = desc;
        Action = action;
    }

    // kept having problems of Action going null, just a sanity check
    public Powerup(Powerup copy) {
        Image = copy.Image;
        Title = copy.Title;
        Description = copy.Description;
        Action = copy.Action;
    }
}


/*
public class Powerups {
    public static Powerup GetRandom => list[Random.Shared.Int(0, list.Length - 1)];
    public static int GetRandomIndex => Random.Shared.Int(0, list.Length - 1);

    public static Powerup GetByIndex(int index) {
        Log.Info("Get Powerup " + index);
        Powerup a = list[Math.Clamp(index, 0, list.Length - 1)];
        Log.Info(a.Action);
        return list[Math.Clamp(index, 0, list.Length - 1)];
    }

    // actions null if they are not instantiated when getted
    public static readonly Powerup[] list = new Powerup[] {
        Damage2,
        FireRate0_05,
        Range50,
        MaxHealth50,
        Heal20,
        Heal40,
        Armor1,
        Leech,
    };

    private static Powerup Damage2 => new(
        "",
        "+2 Damage",
        "Adds 2 damage to selected Goon",
        (p) => p.AddWeaponDamage += 2
    );

    private static Powerup FireRate0_05 => new(
        "",
        "-0.05 Firerate",
        "Fires 0.05 seconds faster for selected Goon",
        (p) => p.AddFireRate -= 0.05f
    );

    private static Powerup Range50 => new(
        "",
        "+50 Range",
        "Adds 50 range to selected Goon",
        (p) => p.AddRange += 50
    );

    private static Powerup MaxHealth50 => new(
        "",
        "+50 Max Health",
        "Adds 50 max health to selected Goon",
        (p) => p.MaxHealth += 50
    );

    private static Powerup Heal20 => new(
        "",
        "Heal 20 health",
        "Heals 20 health to selected Goon",
        (p) => p.Health = Math.Min(p.Health + 20, p.MaxHealth)
    );

    private static Powerup Heal40 => new(
        "",
        "Heal 40 Health",
        "Heals 40 health to selected Goon",
        (p) => p.Health = Math.Min(p.Health + 40, p.MaxHealth)
    );

    private static Powerup Armor1 => new(
        "",
        "+1 Armor",
        "Adds 1 armor to selected Goon",
        (p) => p.Armor++
    );

    private static Powerup Leech => new(
        "",
        "Leech",
        "1/5 chance every attack to regen one health. Every additional leech is another 1/5 chance for another one health",
        (p) => p.AttackActions.Add(p.PowerupLeech)
    );
}
*/
