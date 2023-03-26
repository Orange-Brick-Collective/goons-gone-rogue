using Sandbox;
using System;
using System.Linq;

namespace GGame;

public partial class Pawn : AnimatedEntity {
    public Vector3 HeightOffset => new(0, 0,  35 * Scale);

    public Gun weapon;

    public TimeSince lastFire;

    public bool isInCombat = true;
    public int team = 0;

    [Net] public float MaxHealth {get; set;} = 100;
    [Net] public new float Health {get; set;} = 100;

    public int armor = 0;

    public int moveSpeed = 200;
    public float fireRate = 0.2f;
    public int weaponDamage = 10;

    public float degreeSpread = 2;

    public void OnEnemyKilled() {
		// check boosts
	}

    public void FireGun(Pawn target) {
        if (weapon is null) return;

        TraceResult tr = Trace.Ray(Position + HeightOffset, target.Position + target.HeightOffset)
            .WithoutTags($"team{team}")
            .Run();

        if (lastFire > fireRate) {
            lastFire = 0;
            weapon.Fire(this, tr, weaponDamage, () => {});
        } 
    }
}