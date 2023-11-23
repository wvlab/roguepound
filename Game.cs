using System.Numerics;
using Raylib_CsLo;
using FunctionalRoguePound;

namespace RoguePound;

interface IGameLogic
{
}


public sealed class GameLogic : IGameLogic
{

}


public sealed class Game
{
    Settings Settings = Settings.Instance;
    GameStorage Storage;
    Artist Artist;
    GameLogic Enigmatologist;

    public void Update()
    {
        if (Raylib.IsKeyPressed(KeyboardKey.KEY_UP))
        {
            Storage.Player.Position.Y -= 1;
        }

        if (Raylib.IsKeyPressed(KeyboardKey.KEY_DOWN))
        {
            Storage.Player.Position.Y += 1;
        }

        if (Raylib.IsKeyPressed(KeyboardKey.KEY_LEFT))
        {
            Storage.Player.Position.X -= 1;
        }

        if (Raylib.IsKeyPressed(KeyboardKey.KEY_RIGHT))
        {
            Storage.Player.Position.X += 1;
        }

        if (Raylib.IsKeyPressed(KeyboardKey.KEY_R))
        {
            Storage.Reset();
        }

        if (Raylib.IsKeyPressed(KeyboardKey.KEY_MINUS))
        {
            Storage.Camera.offset -= new Vector2(0.01f, 0.01f);
            Storage.Camera.zoom -= 0.02f;
        }

        if (Raylib.IsKeyPressed(KeyboardKey.KEY_EQUAL))
        {
            Storage.Camera.zoom += 0.01f;
        }

        if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT))
        {
            Vector2 delta = Raylib.GetMouseDelta();
            delta = delta * (-1.0f / Storage.Camera.zoom);

            Storage.Cursor += delta;
        }

        float wheel = Raylib.GetMouseWheelMove();
        if (wheel != 0)
        {
            Vector2 mouseWorldPos = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), Storage.Camera);

            Storage.Camera.offset = Raylib.GetMousePosition();

            Storage.Cursor = mouseWorldPos;

            Storage.Camera.zoom += wheel * 0.125f;
        }

        if (Storage.Camera.zoom < 0.375)
        {
            Storage.Camera.zoom = 0.375f;
        }

        Storage.Camera.target = Storage.Cursor;

        using (Artist.DrawingEnvironment())
        {
            Raylib.ClearBackground(Raylib.BLACK);

            using (Artist.World2DEnvironment(Storage.Camera))
            {
                Artist.DrawDungeon(Storage.Tiles);
                Artist.DrawActor(Storage.Player);
            }
        }
    }

    public Game(Artist artist, GameLogic logic)
    {
        Resolution res = Settings.Instance.Resolution;
        res.InitWindow("RoguePound");
        Storage = new GameStorage();
        Artist = artist;
        Enigmatologist = logic;
    }
}
