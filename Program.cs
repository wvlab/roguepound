using System.Numerics;
using System.Collections.Generic;
using Raylib_CsLo;
using FunctionalRoguePound;

namespace RoguePound;

// TODO: implement movement
// TODO: implement hud
// TODO: implement tile sets
// TODO: implement traps
// TODO: implement enemies && ai
// TODO: implement combat system
// TODO: implement score system
// TODO: implement audio system
// TODO: implement event system
// TODO: implement special theme floors
// TODO: implement statistics
// TODO: implement menus

public static class Program
{
    public static void Main(string[] args)
    {
        Settings settings = Settings.Instance;

        Artist drawer = new Artist();
        GameLogic logic = new GameLogic();
        Game game = new Game(drawer, logic);
        while (!Raylib.WindowShouldClose())
        {
            game.Update();
        }

        Raylib.CloseWindow();
    }
}
