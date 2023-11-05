using System.Numerics;
using System.Collections.Generic;
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
    GameState State;
    Artist Artist;
    GameLogic Enigmatologist;

    public void Update()
    {
        if (Raylib.IsKeyPressed(KeyboardKey.KEY_UP))
        {
            State.Cursor.Y -= Settings.TileSize;
        }

        if (Raylib.IsKeyPressed(KeyboardKey.KEY_DOWN))
        {
            State.Cursor.Y += Settings.TileSize;
        }

        if (Raylib.IsKeyPressed(KeyboardKey.KEY_LEFT))
        {
            State.Cursor.X -= Settings.TileSize;
        }

        if (Raylib.IsKeyPressed(KeyboardKey.KEY_RIGHT))
        {
            State.Cursor.X += Settings.TileSize;
        }

        if (Raylib.IsKeyPressed(KeyboardKey.KEY_R))
        {
            State.Reset();
        }

        if (Raylib.IsKeyPressed(KeyboardKey.KEY_MINUS))
        {
            State.Camera.offset -= new Vector2(0.01f, 0.01f);
            State.Camera.zoom -= 0.02f;
        }

        if (Raylib.IsKeyPressed(KeyboardKey.KEY_EQUAL))
        {
            State.Camera.zoom += 0.01f;
        }

        if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT))
        {
            Vector2 delta = Raylib.GetMouseDelta();
            delta = delta * (-1.0f / State.Camera.zoom);

            State.Cursor += delta;
        }

        float wheel = Raylib.GetMouseWheelMove();
        if (wheel != 0)
        {
            Vector2 mouseWorldPos = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), State.Camera);

            State.Camera.offset = Raylib.GetMousePosition();

            State.Cursor = mouseWorldPos;

            State.Camera.zoom += wheel * 0.125f;
            if (State.Camera.zoom < 0.125f)
            {
                State.Camera.zoom = 0.125f;
            }
        }

        State.Camera.target = State.Cursor;
        Raylib.BeginDrawing();

        Raylib.ClearBackground(Raylib.BLACK);
        Raylib.BeginMode2D(State.Camera);

        Artist.DrawDungeon(State.Tiles);

        Raylib.EndMode2D();

        Raylib.EndDrawing();
    }

    public Game(Artist artist, GameLogic logic)
    {
        Resolution res = Settings.Instance.Resolution;
        res.InitWindow("RoguePound");
        State = new GameState();
        Artist = artist;
        Enigmatologist = logic;
    }
}
