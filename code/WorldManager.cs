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

    public void Generatelevel(string[] tiles, int depth) {

    }
}

