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
        State.HandleInput();
        Enigmatologist.UpdateFogOfWar();
        Enigmatologist.CheckLevelup();

        using (Artist.DrawingEnvironment())
        {
            Raylib.ClearBackground(Raylib.BLACK);

            using (Artist.World2DEnvironment(GameStorage.Camera))
            {
                Artist.DrawDungeon(GameStorage.Tiles);
                Artist.DrawInteractiveObjects(GameStorage.InteractiveObjects, GameStorage.Tiles);
                Artist.DrawActor(GameStorage.Player);
                foreach (MonsterData MData in GameStorage.Monsters)
                {
                    Artist.DrawActor(MData.Monster);
                }
            }
            Artist.DrawStatusBar();
        }
    }

    static public void Begin()
    {
        Settings.Resolution.InitWindow("RoguePound");
        GameStorage.Reset();
    }
}

