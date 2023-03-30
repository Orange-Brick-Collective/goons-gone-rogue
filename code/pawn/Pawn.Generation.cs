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

    public void Generate() {
        Name = PawnNames[Random.Shared.Int(0, PawnNames.Length - 1)] + " " + PawnSurnames[Random.Shared.Int(0, PawnSurnames.Length - 1)];

        // outift

        // hat

        // other accessory

        // ticket system for base stats
    }
}