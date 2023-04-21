using Sandbox;

namespace GGame;

public static class WallModels {
    public static int RandomWall() {
        return System.Random.Shared.Int(0, 3);
    }

    public static string[] GetModels(int i) {
        return i switch {
            1 => level1,
            2 => level2,
            3 => level3,
            _ => level0,
        };
    }

    public static readonly string[] level0 = {
        "models/map/walls/wall0-full.vmdl",
        "models/map/walls/wall0-half.vmdl",
        "models/map/walls/wall0-half.vmdl",
        "models/map/walls/wall0-flat.vmdl",
        "models/map/walls/wall0-flat.vmdl",
        "models/map/walls/wall0-flat.vmdl",
    };

    public static readonly string[] level1 = {
        "models/map/walls/wall1-full.vmdl",
        "models/map/walls/wall1-half.vmdl",
        "models/map/walls/wall1-half.vmdl",
        "models/map/walls/wall1-flat.vmdl",
        "models/map/walls/wall1-flat.vmdl",
        "models/map/walls/wall1-flat.vmdl",
    };

    public static readonly string[] level2 = {
        "models/map/walls/wall2-full.vmdl",
        "models/map/walls/wall2-half.vmdl",
        "models/map/walls/wall2-half.vmdl",
        "models/map/walls/wall2-flat.vmdl",
        "models/map/walls/wall2-flat.vmdl",
        "models/map/walls/wall2-flat.vmdl",
    };

    public static readonly string[] level3 = {
        "models/map/walls/wall3-full.vmdl",
        "models/map/walls/wall3-half.vmdl",
        "models/map/walls/wall3-half.vmdl",
        "models/map/walls/wall3-flat.vmdl",
        "models/map/walls/wall3-flat.vmdl",
        "models/map/walls/wall3-flat.vmdl",
    };
}