using Sandbox;
using System;

namespace GGame;

public partial class Pawn : AnimatedEntity {
    public string[] hats = new[] {
        "models/hats/hat1.vmdl",
        "models/hats/hat2.vmdl",
    };

    public string[] PawnNames = new[] {
        "Arduino",
        "Adriano",
        "Alfredo",
        "Bill",
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

    public void Generate(float depth = 0) {
        Name = PawnNames[Random.Shared.Int(0, PawnNames.Length - 1)] + " " + 
            PawnSurnames[Random.Shared.Int(0, PawnSurnames.Length - 1)];

        Scale = Random.Shared.Float(0.8f, 1.1f);

        if (Team == 0) {
            RenderColor = Random.Shared.Float(0, 1) > 0.5f ? new Color(0.3f, 0.5f, 0.3f) : new Color(0.3f, 0.3f, 0.5f);
        } else {
            RenderColor = new Color(0.5f, 0.3f, 0.3f);
        }
        
		int random = Random.Shared.Int(hats.Length);
		if (random != hats.Length) {
			_ = new ModelEntity(hats[random], this);
		}


        // outift

        // hat

        // other accessory

        // * tickets
        // 2 ticket per health
        // 4 ticket per armor
        // 1 ticket per movespeed
        // 5 ticket per weapondamage
        // 5 ticket per -0.02 firerate
        // 5 ticket per -0.1 reloadtime
        // 2 ticket per magazinesize
        // 3 ticket per -0.1 degreespread
        // 1 ticket per 2 range
        float maxValue = 25 * (1 + (depth * 0.3f));

        for (int i = 0; i < 9; i++) {
            float weighted = GenerateWeighted();
            float change = maxValue * weighted;

            switch (i) {
                case 0: {
                    MaxHealth += change * 0.5f;
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
                    AddWeaponDamage += (int)(change * 0.2f);
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