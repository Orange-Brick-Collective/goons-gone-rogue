using Sandbox;
using System;

namespace GGame;

public partial class Pawn : AnimatedEntity {
    public enum GoonType {
        Normal,
        Melee,
        Swarm,
        Boss,
    }

    public string[] hats = new[] {
        "models/hats/hat1.vmdl",
        "models/hats/hat2.vmdl",
    };

    public string[] PawnNames = new[] {
        "Adriano",
        "Alfredo",
        "Bob",
        "Bianci",
        "Bernardo",
        "Cesare",
        "Constanze",
        "Dario",
        "Ferdinando",
        "Francesco",
        "Giacomo",
        "Giovanni",
        "Ines",
        "Napoleon",
        "Paolo",
        "Rocco",
        "Santo",
        "Vittorio",
    };

    public string[] PawnSurnames = new[] {
        "Abate",
        "Acqua",
        "Aiello",
        "Albero",
        "Bianchi",
        "Carpenteri",
        "Di Genova",
        "Esposito",
        "Ferrari",
        "Ferraro",
        "Franco",
        "Gambino",
        "Giorgio",
        "Greco",
        "Lombardo",
        "Marino",
        "Muratori",
        "Napolitano",
        "Romano",
        "Rossi",
        "Russo",
        "Smith",
        "Zello",
    };

    public void Generate(float depth, GoonType type) {
        Name = $"{PawnNames[Random.Shared.Int(0, PawnNames.Length - 1)]} {PawnSurnames[Random.Shared.Int(0, PawnSurnames.Length - 1)]}";

        Scale = Random.Shared.Float(0.68f, 0.92f);
        SetupPhysicsFromAABB(PhysicsMotionType.Keyframed, new Vector3(-16, -16, 0), new Vector3(16, 16, 76));

        // color
        if (Team == 0) {
            RenderColor = Random.Shared.Int(0, 3) switch {
                1 => new Color(0.3f, 0.3f, 0.5f),
                2 => new Color(0.1f, 0.4f, 0.4f),
                3 => new Color(0.3f, 0.7f, 0.5f),
                _ => new Color(0.3f, 0.5f, 0.3f),
            };
        } else {
            RenderColor = Random.Shared.Int(0, 3) switch {
                1 => new Color(0.5f, 0.3f, 0.3f),
                2 => new Color(0.5f, 0.4f, 0.3f),
                3 => new Color(0.7f, 0.3f, 0.4f),
                _ => new Color(0.5f, 0.1f, 0.1f),
            };
        }
        
        // hat
		int random = Random.Shared.Int(hats.Length);
		if (random != hats.Length) {
			_ = new ModelEntity(hats[random], this);
		}

        // other accessory

        // * tickets
        // 1 ticket per health              4 ticket per armor
        // 1 ticket per movespeed           6 ticket per weapondamage
        // 5 ticket per -0.02 firerate      5 ticket per -0.1 reloadtime
        // 2 ticket per magazinesize        3 ticket per -0.1 degreespread
        // 1 ticket per 2 range
        
        float baseTicket = 25;
        if (type is GoonType.Swarm) {
            baseTicket = 10;
            Scale = Random.Shared.Float(0.64f, 0.72f);
			BaseWeaponDamage = 1;
			MaxHealth -= 50;
			Health -= 50;
        } else if (type is GoonType.Boss) {
            baseTicket = 60;
            Scale = Random.Shared.Float(2.2f, 2.6f);
            BaseWeaponDamage = 2;
            BaseFireRate = 0.18f;
            MaxHealth += 300;
            Health += 300;
        }

        float maxTicket = baseTicket * (1 + (depth * 0.2f));

        for (int i = 0; i < 9; i++) {
            float weighted = GenerateWeighted();
            float change = maxTicket * weighted;

            switch (i) {
                case 0: {
                    MaxHealth += change;
                    Health = MaxHealth;
                    break;
                }
                case 1: {
                    Armor += (int)(change * 0.25f);
                    break;
                }
                case 2: {
                    AddMoveSpeed += (int)change;
                    break;
                }
                case 3: {
                    AddWeaponDamage += (int)(change * 0.16f);
                    break;
                }
                case 4: {
                    AddFireRate -= 0.02f * (int)(change * 0.2f);
                    break;
                }
                case 5: {
                    AddReloadTime -= 0.1f * (int)(change * 0.2f);
                    break;
                }
                case 6: {
                    AddMagazineSize += (int)(change * 0.5f);
                    break;
                }
                case 7: {
                    AddDegreeSpread -= 0.1f * (int)(change * 0.334f);
                    break;
                }
                case 8: {
                    AddRange += change * 2;
                    break;
                }
            }
        }
    }

    public float GenerateWeighted() {
        float num = 1;
        for (int i = 0; i < 2; i++) {
            float newNum = Random.Shared.Float(0, 1);
            if (newNum < num) num = newNum;
        }
        return num;
    }
}