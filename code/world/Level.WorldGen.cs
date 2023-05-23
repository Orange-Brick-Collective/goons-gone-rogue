using Sandbox;
using System;
using System.Collections.Generic;

namespace GGame;

public class World {
    public int length = 10, width = 6;
    public int depth = 0;
    public int wallType = 0;
    public List<List<Tile>> tiles;
    public Transform startPos, endPos;
}

public class WorldGen {
    public static WorldGen Current {get; set;}
    
    public WorldGen() {
        if (Current is not null) return;
        Current = this;
    }

    public async System.Threading.Tasks.Task GenerateWorld(int len, int wid, int depth, int? wall = null) {
        Game.AssertServer();
        Log.Info("Generating World");

        foreach (Entity ent in Entity.All) {
            if (ent.Tags.Has("generated")) ent.Delete();
        }

        World lvl = new() {
            length = len,
            width = wid,
            depth = depth,
            wallType = wall ?? WallModels.RandomWall(),
            tiles = FillTilesEmpty(len, wid),
        };

        len -= 1;
        wid -= 1;

        // * grid stage
        Vector2 startP = new(0, Random.Shared.Int(0, wid));
        Vector2 endP = new(len, Random.Shared.Int(0, wid));
        List<List<bool>> gridRoads = FillGridEmpty(len + 1, wid + 1);

        if (IsBossDepth(depth)) {
            startP = new(0, 2);
            endP = new(8, 2);
            MarkRoad(gridRoads, startP, endP, len, wid, false);

            GenerateWorldGridBoss(ref lvl, ref gridRoads, startP, endP);

            GenerateRoads(ref lvl, ref gridRoads);
            await GameTask.DelayRealtime(20);

            GenerateWalls(ref lvl);
            await GameTask.DelayRealtime(20);

            GenerateEventsBoss(ref lvl, startP, endP);
            await GameTask.DelayRealtime(20);

            GenerateProps(ref lvl);
            await GameTask.DelayRealtime(20);
        } else {
            MarkRoad(gridRoads, startP, endP, len, wid, true);

            GenerateWorldGrid(ref lvl, ref gridRoads, startP, endP);

            GenerateRoads(ref lvl, ref gridRoads);
            await GameTask.DelayRealtime(20);

            GenerateWalls(ref lvl);
            await GameTask.DelayRealtime(20);

            GenerateEvents(ref lvl, startP, endP);
            await GameTask.DelayRealtime(20);

            GenerateProps(ref lvl);
            await GameTask.DelayRealtime(20);
        }



        GGame.Current.currentWorld = lvl;
        return;
    }

    public static void GenerateWorldGrid(ref World lvl, ref List<List<bool>> gridRoads, Vector2 startP, Vector2 endP) {
        int len = lvl.length - 1, wid = lvl.width - 1;

        foreach (Vector2 curP in MakeBranches(lvl, len, wid, startP, endP)) {
            Vector2 nearestP = new(len * 0.5f, wid * 0.5f);
            for (int i = 0; i < 50; i++) {
                int curX = (int)curP.x;
                int curY = (int)curP.y;
                int dirX = (int)Math.Clamp(curP.x + i * 0.8f, 0, len);
                int dirY = (int)Math.Clamp(curP.y + i * 0.8f, 0, wid);
                int negX = (int)Math.Clamp(curP.x - i * 0.8f, 0, len);
                int negY = (int)Math.Clamp(curP.y - i * 0.8f, 0, wid);

                // YEA ITS BAD, can be improved but idc
                if (gridRoads[dirX][curY]) nearestP = new(dirX, curY); // N
                if (gridRoads[curX][dirY]) nearestP = new(curX, dirY); // E
                if (gridRoads[negX][curY]) nearestP = new(negX, curY); // S
                if (gridRoads[curX][negY]) nearestP = new(curX, negY); // W

                if (gridRoads[dirX][dirY]) nearestP = new(dirX, dirY); // NE
                if (gridRoads[negX][dirY]) nearestP = new(negX, dirY); // SE
                if (gridRoads[negX][negY]) nearestP = new(negX, negY); // SW
                if (gridRoads[dirX][negY]) nearestP = new(dirX, negY); // NW

                if (nearestP != new Vector2(len * 0.5f, wid * 0.5f)) break;
            }

            MarkRoad(gridRoads, curP, nearestP, len, wid, true);
        }
    }

    private static void GenerateRoads(ref World lvl, ref List<List<bool>> gridRoads) {
        int len = lvl.length - 1, wid = lvl.width - 1;

        for (int l = 0; l <= len; l++) for (int w = 0; w <= wid; w++) {
            if (gridRoads[l][w]) {
                bool[] dir = {false, false, false, false};
                int connected = 0;

                if (l + 1 <= len && gridRoads[l + 1][w]) dir[0] = true;
                if (w + 1 <= wid && gridRoads[l][w + 1]) dir[1] = true;
                if (l - 1 >= 0 && gridRoads[l - 1][w]) dir[2] = true;
                if (w - 1 >= 0 && gridRoads[l][w - 1]) dir[3] = true;
                foreach (bool e in dir) if (e) connected += 1;

                // ignore rotation numbers, the model withm them doesnt make much sense
                switch (connected) {
                    case 1: {
                        for (int i = 0; i < 4; i++) {
                            if (dir[i]) {
                                Tile(lvl, TileType.Endcap, new Vector2(l, w), i);
                                break;
                            }
                        }
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
                            Tile(lvl, TileType.T, new Vector2(l, w), dir[1] ? 2 : 0);
                        } else {
                            Tile(lvl, TileType.T, new Vector2(l, w), dir[2] ? 3 : 1);
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

        foreach (Entity ent in Entity.All) {
            if (ent is TileEmpty) ent.Delete();
        }
    }

    public static void GenerateWalls(ref World lvl) {
        for (int l = 0; l <= lvl.length - 1; l++) for (int w = 0; w <= lvl.width - 1; w++) {
            lvl.tiles[l][w].MakeWalls(lvl.wallType);
        }
    }

    public static void GenerateEvents(ref World lvl, Vector2 startP, Vector2 endP) {
        int shops = 0;

        new TileEventStart().Init(lvl.tiles[(int)startP.x][(int)startP.y]);
        lvl.startPos = lvl.tiles[(int)startP.x][(int)startP.y].Transform;
        
        new TileEventEnd().Init(lvl.tiles[(int)endP.x][(int)endP.y]);
        lvl.endPos = lvl.tiles[(int)endP.x][(int)endP.y].Transform;

        for (int l = 0; l <= lvl.length - 1; l++) for (int w = 0; w <= lvl.width - 1; w++) {
            Tile tile = lvl.tiles[l][w];
            if (tile is TileEmpty) continue;

            Vector2 e = new(l, w);
            if (e == startP || e == endP) continue;

            if (tile is TileEnd || GreaterThan(97.5f)) {
                new TileEventPowerups().Init(tile);
                continue;
            }

            if (tile is TileStraight) {
                float increase = Math.Min(lvl.depth, 14f);
                if (GreaterThan(60 - increase)) { // 60 at 0 depth, 40 at 20 depth
                    if (GreaterThan(8)) {
                        new TileEventFight().Init(tile);
                    } else {
                        new TileEventSwarm().Init(tile);
                    }
                    continue;
                }
            } else {
                float increase = Math.Min(lvl.depth, 14f);
                if (GreaterThan(72 - increase)) { // 72 at 0 depth, 52 at 20 depth
                    if (GreaterThan(8)) {
                        new TileEventFight().Init(tile);
                    } else {
                        new TileEventSwarm().Init(tile);
                    }
                    continue;
                }
            }

            if (tile is TileT && GreaterThan(80f + shops * 4)) {
                new TileEventShop().Init(tile);
                shops++;
                continue;
            }
        }
    }

    public static void GenerateProps(ref World lvl) {
        for (int l = 0; l <= lvl.length - 1; l++) for (int w = 0; w <= lvl.width - 1; w++) {
            if (GreaterThan(66) && lvl.tiles[l][w].tileEvent is null) {
                lvl.tiles[l][w].MakeLamp();
            }
        }
    }

    // ! GEN BOSS FUNCTIONS

    public static void GenerateWorldGridBoss(ref World lvl, ref List<List<bool>> gridRoads, Vector2 startP, Vector2 endP) {
        int len = lvl.length - 1, wid = lvl.width - 1;

        foreach (Vector2 curP in new List<Vector2>() {new(7, 1), new(7, 3), new(5, 1), new(5, 3)}) {
            Vector2 nearestP = new(len * 0.5f, wid * 0.5f);
            for (int i = 0; i < 50; i++) {
                int curX = (int)curP.x;
                int curY = (int)curP.y;
                int dirX = (int)Math.Clamp(curP.x + i * 0.8f, 0, len);
                int dirY = (int)Math.Clamp(curP.y + i * 0.8f, 0, wid);
                int negX = (int)Math.Clamp(curP.x - i * 0.8f, 0, len);
                int negY = (int)Math.Clamp(curP.y - i * 0.8f, 0, wid);

                // YEA ITS BAD, can be improved but idc
                if (gridRoads[dirX][curY]) nearestP = new(dirX, curY); // N
                if (gridRoads[curX][dirY]) nearestP = new(curX, dirY); // E
                if (gridRoads[negX][curY]) nearestP = new(negX, curY); // S
                if (gridRoads[curX][negY]) nearestP = new(curX, negY); // W

                // if (gridRoads[dirX][dirY]) nearestP = new(dirX, dirY); // NE
                // if (gridRoads[negX][dirY]) nearestP = new(negX, dirY); // SE
                // if (gridRoads[negX][negY]) nearestP = new(negX, negY); // SW
                // if (gridRoads[dirX][negY]) nearestP = new(dirX, negY); // NW

                if (nearestP != new Vector2(len * 0.5f, wid * 0.5f)) break;
            }

            MarkRoad(gridRoads, curP, nearestP, len, wid, true);
        }
    }

    public static void GenerateEventsBoss(ref World lvl, Vector2 startP, Vector2 endP) {
        new TileEventStart().Init(lvl.tiles[(int)startP.x][(int)startP.y]);
        lvl.startPos = lvl.tiles[(int)startP.x][(int)startP.y].Transform;
        
        new TileEventEnd().Init(lvl.tiles[(int)endP.x][(int)endP.y]);
        lvl.endPos = lvl.tiles[(int)endP.x][(int)endP.y].Transform;

        new TileEventBoss().Init(lvl.tiles[3][2]);
        new TileEventShop().Init(lvl.tiles[5][2]);

        for (int l = 0; l <= lvl.length - 1; l++) for (int w = 0; w <= lvl.width - 1; w++) {
            Tile tile = lvl.tiles[l][w];
            if (tile is TileEmpty) continue;

            Vector2 e = new(l, w);
            if (e == startP || e == endP) continue;

            if (tile is TileEnd) {
                new TileEventPowerups().Init(tile);
                continue;
            }
        }
    }

    // * 
    // * other
    // * 

    private static bool IsBossDepth(int depth) {
        if (depth != 0 && depth % 5 == 0) return true;
        else return false;
    }

    private static bool GreaterThan(float percentComparison) {
        return Random.Shared.Float(0, 100) > percentComparison;
    }

    private static void MarkRoad(List<List<bool>> grid, Vector2 startP, Vector2 endP, int len, int wid, bool detour) {
        bool making = true;
        int i = 0, lastDetour = 1;
        Vector2 curP = startP;
        Vector3 dir = new((endP - curP).Normal, 0);

        grid[(int)startP.x][(int)startP.y] = true;
        grid[(int)endP.x][(int)endP.y] = true;

        while (making) {
            i++; lastDetour++;
            
            // detour
            if (detour) {
                if (lastDetour > 6 && Random.Shared.Float(0, 1) > 0.5f) {
                    if (Vector2.Distance(curP, endP) < 2) continue;
                    
                    dir *= Rotation.FromYaw(Random.Shared.Float(-80, 80));
                    lastDetour = 0;
                } else if (lastDetour > 2) {
                    dir = new((endP - curP).Normal, 0);
                }
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

            curP = newP;
            if (i > 100 || FloorVec2(curP) == endP) {
                making = false;
            }
        }
    }



    private static Vector2 MakeVec2(Vector3 vec) {
        return new Vector2(vec.x, vec.y);
    }
    private static Vector2 FloorVec2(Vector3 vec) {
        return new Vector2((int)vec.x, (int)vec.y);
    }

    private static void Tile(World lvl, TileType type, Vector2 pos, int rot = 0, Color? color = null) {
        var (x, y) = ((int)pos.x, (int)pos.y);
        if (lvl.tiles[x][y] is not TileEmpty) return;

        switch (type) {
            case TileType.Endcap: {
                lvl.tiles[x][y] = new TileEnd(new Vector2(x, y) * 512, rot) {
                    RenderColor = color ?? Color.White,
                };
                break;
            }
            case TileType.Straight: {
                lvl.tiles[x][y] = new TileStraight(new Vector2(x, y) * 512, rot) {
                    RenderColor = color ?? Color.White,
                };
                break;
            }
            case TileType.Corner: {
                lvl.tiles[x][y] = new TileCorner(new Vector2(x, y) * 512, rot) {
                    RenderColor = color ?? Color.White,
                };
                break;
            }
            case TileType.T: {
                lvl.tiles[x][y] = new TileT(new Vector2(x, y) * 512, rot) {
                    RenderColor = color ?? Color.White,
                };
                break;
            }
            case TileType.X: {
                lvl.tiles[x][y] = new TileX(new Vector2(x, y) * 512, 0) {
                    RenderColor = color ?? Color.White,
                };
                break;
            }
        }
    }

    private static List<Vector2> MakeBranches(World lvl, int l, int w, Vector2 startP, Vector2 endP) {
        List<Vector2> branches = new();
        for (int i = 0; i < ((byte)Random.Shared.Int(3 + lvl.depth, 5 + lvl.depth)); i++) {
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
