using Sandbox;
using System;
using System.Collections.Generic;

namespace GGame;

public class WorldManager {
    public static WorldManager Cur {get; set;}
    public Level currentLevel; 

    public WorldManager() {
        if (Cur is not null) return;
        Cur = this;
    }

    [ConCmd.Server("gen")]
    public static void GenerateLevelCMD() {
        Cur.GenerateLevel(8, 6, 0);
    }

    public void GenerateLevel(int length, int width, int depth) {
        Log.Info("Generating");

        foreach (Entity ent in Entity.All) {
            if (ent.Tags.Has("tile")) ent.Delete();
        }

        Level lvl = new() {
            depth = depth,
            wallType = Random.Shared.Int(0, 3),
            tiles = new(),
        };

        Vector2 startPoint = new(0, Random.Shared.Int(0, width));
        Vector2 endPoint = new(length, Random.Shared.Int(0, width));

        for (int l = 0; l < length; l++) {
            lvl.tiles.Add(new List<Tile>());
            for (int w = 0; w < width; w++) {

                switch(Random.Shared.Int(0, 3)) {
                    case 0: {
                        lvl.tiles[l].Add(new TileStraight(new Vector3(l * 512, w * 512), ""));
                        break;
                    }
                    case 1: {
                        lvl.tiles[l].Add(new TileCorner(new Vector3(l * 512, w * 512), ""));
                        break;
                    }
                    case 2: {
                        lvl.tiles[l].Add(new TileT(new Vector3(l * 512, w * 512), ""));
                        break;
                    }
                    case 3: {
                        lvl.tiles[l].Add(new TileX(new Vector3(l * 512, w * 512), ""));
                        break;
                    }
                }
            }
        }

        currentLevel = lvl;
    }
}

