using Sandbox;
using System;

namespace GGame;

public partial class Pawn : AnimatedEntity {
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

    public void Generate(float difficulty) {
        Name = PawnNames[Random.Shared.Int(0, PawnNames.Length - 1)] + " " + PawnSurnames[Random.Shared.Int(0, PawnSurnames.Length - 1)];

        Scale = Random.Shared.Float(0.6f, 1.2f);

        // outift

        // hat

        // other accessory

        float tickets = 200 * difficulty;

        // ticket system for base stats
    }
}