using System.Numerics;
using Raylib_CsLo;
using FunctionalRoguePound;
using FunctionalRoguePound.FUtility;

namespace RoguePound;


public static class Game
{
    static GameState State = new();
    static public void Update()
    {
        if (GameStorage.Player.Stats.Health <= 0)
        {
            if (!Raylib.IsKeyPressed(KeyboardKey.KEY_R))
            {
                Artist.DrawGameOver();
                return;
            }
            GameStorage.Reset();
        }
        State.HandleInput();
        Enigmatologist.UpdateFogOfWar();
        Enigmatologist.CheckLevelup();
        Artist.DrawGame();
    }

    static public void Begin()
    {
        Settings.Resolution.InitWindow("RoguePound");
        GameStorage.Reset();
    }
}

