using Sandbox;
using System;

namespace GGame;

public class Powerups {
    public static Powerup GetRandom => list[Random.Shared.Int(0, list.Length - 1)];
    public static int GetRandomIndex => Random.Shared.Int(0, list.Length - 1);

    public static Powerup GetByIndex(int index) {
        return list[Math.Clamp(index, 0, list.Length - 1)];
    }

    public static Powerup[] list = new Powerup[] {
        new Powerup("", "+2 Damage", "Adds 2 damage to selected Goon", (p) => 
            p.AddWeaponDamage += 2
        ),
        new Powerup("", "-0.05 Firerate", "Fires 0.05 seconds faster for selected Goon", (p) => 
            p.AddFireRate -= 0.05f
        ),
        new Powerup("", "+50 Range", "Adds 50 range to selected Goon", (p) => 
            p.AddRange += 50
        ),
        new Powerup("", "+50 Max Health", "Adds 50 max health to selected Goon", (p) => 
            p.MaxHealth += 50
        ),
        new Powerup("", "Heal 20 health", "Heals 20 health to selected Goon", (p) => 
            p.Health = Math.Min(p.Health + 20, p.MaxHealth)
        ),
        new Powerup("", "Heal 40 Health", "Heals 40 health to selected Goon", (p) => 
            p.Health = Math.Min(p.Health + 40, p.MaxHealth)
        ),
        new Powerup("", "+1 Armor", "Adds 1 armor to selected Goon", (p) => 
            p.Armor++
        ),
        new Powerup("", "Leech", "1/5 chance every attack to regen one health. Every additional leech is another 1/5 chance for another one health", (p) => 
            p.AttackActions.Add(p.PowerupLeech)
        ),
    };
}

public class Powerup {
    public string Image {get; set;} = "unset";
    public string Title {get; set;} = "unset";
    public string Description {get; set;} = "unset";
    public Action<Pawn> Action {get; set;}

    public Powerup(string image, string title, string desc, Action<Pawn> action) {
        Image = image;
        Title = title;
        Description = desc;
        Action = action;
    }
}