using Raylib_CsLo;

namespace RoguePound;

// TODO: implement hud
// TODO: implement traps
// TODO: implement enemies && ai
// TODO: implement combat system
// TODO: implement audio system
// TODO: implement special theme floors
// TODO: implement statistics
// TODO: implement menus

public static class Program
{
    public static void Main(string[] args)
    {
        Game.Begin();

        while (!Raylib.WindowShouldClose())
        {
            Game.Update();
        }

        Raylib.CloseWindow();
    }
}
