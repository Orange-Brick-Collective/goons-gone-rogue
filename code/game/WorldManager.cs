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
        Cur.GenerateLevel(14, 12, 0, true);
    }

    public async void GenerateLevel(int len, int wid, int depth, bool debug) {
        Log.Info("Generating");

        foreach (Entity ent in Entity.All) {
            if (ent.Tags.Has("tile")) ent.Delete();
        }

        Level lvl = new() {
            length = len,
            width = wid,
            depth = depth,
            wallType = Random.Shared.Int(0, 3),
            tiles = FillTilesEmpty(len, wid),
        };

        len -= 1;
        wid -= 1;

        await GameTask.DelayRealtime(50);
        if (debug) await GameTask.DelayRealtime(250);

        // *
        // * grid stage
        // *
        Vector2 startP = new(0, Random.Shared.Int(0, wid));
        Vector2 endP = new(len, Random.Shared.Int(0, wid));
        List<Vector2> branchP = MakeBranches(len, wid, startP, endP);
        List<List<bool>> gridRoads = FillGridEmpty(len + 1, wid + 1);

        MarkRoad(gridRoads, startP, endP, len, wid, debug);

        if (debug) {
            for (int l = 0; l <= len; l++) for (int w = 0; w <= wid; w++) {
                DebugOverlay.Sphere(new Vector3(l * 512, w * 512, 0), 60, gridRoads[l][w] ? Color.Green : Color.Red, 2, false);
            }
        }

        // foreach (Vector2 curP in branchP) {
            // find nearest road
            // MarkRoad(gridRoads, curP, nearestP, l ,w);
        // }

        await GameTask.DelayRealtime(50);
        if (debug) await GameTask.DelayRealtime(450);

        // *
        // * road stage
        // *
        GenerateRoads(lvl, gridRoads, len, wid);

        if (debug) {
            DebugOverlay.Sphere(startP * 512, 100, Color.White, 2, false);
            DebugOverlay.Sphere(endP * 512, 100, Color.White, 2, false);
        }

        await GameTask.DelayRealtime(50);
        if (debug) await GameTask.DelayRealtime(450);

        // *
        // * walls stage
        // *
        // for (int l = 0; l < length; l++) for (int w = 0; w < width; w++) {
        // }

        await GameTask.DelayRealtime(50);
        if (debug) await GameTask.DelayRealtime(450);

        // *
        // * props stage
        // *
        // for (int l = 0; l < length; l++) for (int w = 0; w < width; w++) {
        // }

        await GameTask.DelayRealtime(50);
        if (debug) await GameTask.DelayRealtime(450);

        // *
        // * cleanup stage
        // *
        foreach (Entity ent in Entity.All) {
            if (ent is TileEmpty) ent.Delete();
        }

        currentLevel = lvl;
    }

    private static void MarkRoad(List<List<bool>> grid, Vector2 startP, Vector2 endP, int len, int wid, bool debug = false) {
        bool making = true;
        int i = 0, lastDetour = 1;
        Vector2 curP = startP;
        Vector3 dir = new((endP - curP).Normal, 0);

        grid[(int)startP.x][(int)startP.y] = true;
        grid[(int)endP.x][(int)endP.y] = true;

        while (making) {
            i++; lastDetour++;
            
            // detour
            if (lastDetour > 6 && Random.Shared.Float(0, 1) > 0.5f) {
                if (Vector2.Distance(curP, endP) < 2) continue;
                
                dir *= Rotation.FromYaw(Random.Shared.Float(-80, 80));
                lastDetour = 0;
            } else if (lastDetour > 2) {
                dir = new((endP - curP).Normal, 0);
            }
            
            Vector2 newP = curP + MakeVec2(dir);
            newP.x = newP.x.Clamp(0, len);
            newP.y = newP.y.Clamp(0, wid);

            int xOffset = (int)newP.x - (int)curP.x;
            int yOffset = (int)newP.y - (int)curP.y;

            if (xOffset != 0 && yOffset != 0) {
                grid[(int)curP.x + xOffset][(int)curP.y] = true;
            }

            grid[(int)newP.x][(int)newP.y] = true;

            if (debug) {
                DebugOverlay.Line(curP * 512, newP * 512, Color.White, 2, false);
                DebugOverlay.Line(FloorVec2(curP) * 512, FloorVec2(newP) * 512, Color.Blue, 2, false);
            }

            curP = newP;
            if (i > 30 || FloorVec2(curP) == endP) {
                making = false;
            }
        }
    }

    private static void GenerateRoads(Level lvl, List<List<bool>> gridRoads, int len, int wid) {
        for (int l = 0; l <= len; l++) for (int w = 0; w <= wid; w++) {
            if (gridRoads[l][w]) {
                bool[] dir = {false, false, false, false};
                int connected = 0;

                if (l + 1 <= len && gridRoads[l + 1][w]) dir[0] = true;
                if (w + 1 <= wid && gridRoads[l][w + 1]) dir[1] = true;
                if (l - 1 >= 0   && gridRoads[l - 1][w]) dir[2] = true;
                if (w - 1 >= 0   && gridRoads[l][w - 1]) dir[3] = true;
                foreach (bool e in dir) if (e) connected += 1;

                // ignore rotation numbers, the model vs them doesnt make much sense
                switch (connected) {
                    case 1: {
                        // NO ENDCAP PIECE
                        break;
                    }
                    case 2: {
                        if (dir[0] && dir[2] || dir[1] && dir[3]) {
                            Tile(lvl, TileType.Straight, new Vector2(l, w), dir[0] ? 0 : 1);
                        } else {
                            if (dir[0]) {
                                Tile(lvl, TileType.Corner, new Vector2(l, w), dir[1] ? 1 : 0);
                            } else {
                                Tile(lvl, TileType.Corner, new Vector2(l, w), dir[3] ? 3 : 2);
                            }
                        }
                        break;
                    } 
                    case 3: {
                        if (dir[0] && dir[2]) {
                            Tile(lvl, TileType.T, new Vector2(l, w), dir[1] ? 0 : 2);
                        } else {
                            Tile(lvl, TileType.T, new Vector2(l, w), dir[2] ? 1 : 3);
                        }
                        break;
                    } 
                    case 4: {
                        Tile(lvl, TileType.X, new Vector2(l, w));
                        break;
                    } 
                }
            }
        }
    }

    private static Vector2 MakeVec2(Vector3 vec) {
        return new Vector2(vec.x, vec.y);
    }
    private static Vector2 FloorVec2(Vector3 vec) {
        return new Vector2((int)vec.x, (int)vec.y);
    }

    private static void Tile(Level lvl, TileType type, Vector2 pos, int rot = 0, Color? color = null) {
        var (x, y) = ((int)pos.x, (int)pos.y);
        if (lvl.tiles[x][y] is not TileEmpty) return;

        switch (type) {
            case TileType.Endcap: {
                // lvl.tiles[x][y] = new TileStraight(new Vector2(x, y) * 512, "", rot) {
                //     RenderColor = color ?? Color.White,
                // };
                break;
            }
            case TileType.Straight: {
                lvl.tiles[x][y] = new TileStraight(new Vector2(x, y) * 512, "", rot) {
                    RenderColor = color ?? Color.White,
                };
                break;
            }
            case TileType.Corner: {
                lvl.tiles[x][y] = new TileCorner(new Vector2(x, y) * 512, "", rot) {
                    RenderColor = color ?? Color.White,
                };
                break;
            }
            case TileType.T: {
                lvl.tiles[x][y] = new TileT(new Vector2(x, y) * 512, "", rot) {
                    RenderColor = color ?? Color.White,
                };
                break;
            }
            case TileType.X: {
                lvl.tiles[x][y] = new TileX(new Vector2(x, y) * 512, "", 0) {
                    RenderColor = color ?? Color.White,
                };
                break;
            }
        }
    }

    private static List<Vector2> MakeBranches(int l, int w, Vector2 startP, Vector2 endP) {
        List<Vector2> branches = new();
        for (int i = 0; i < ((byte)Random.Shared.Int(3, 6)); i++) {
            Vector2 pos = new(Random.Shared.Int(0, l), Random.Shared.Int(0, w));
    
            while (pos == startP || pos == endP) {
                pos = new(Random.Shared.Int(0, l), Random.Shared.Int(0, w));
            }

            branches.Add(pos);
        }
        return branches;
    }

    private static List<List<Tile>> FillTilesEmpty(int l, int w) {
        List<List<Tile>> tiles = new();
        for (int len = 0; len < l; len++) {
            tiles.Add(new List<Tile>());

            for (int wid = 0; wid < w; wid++) {
                tiles[len].Add(new TileEmpty());
            }
        }
        return tiles;
    }

    private static List<List<bool>> FillGridEmpty(int l, int w) {
        List<List<bool>> grid = new();
        for (int len = 0; len < l; len++) {
            grid.Add(new List<bool>());

            for (int wid = 0; wid < w; wid++) {
                grid[len].Add(false);
            }
        }
        return grid;
    }
}
