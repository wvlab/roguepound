using System.Numerics;
using Raylib_CsLo;
using FunctionalRoguePound;
using FunctionalRoguePound.FUtility;

namespace RoguePound;

interface IState
{
    void HandleInput();
    void Reset();
}


record class MovementState(GameStorage Storage) : IState
{
    bool isRunning = false;

    public void HandleInput()
    {
        // TODO: Check for boundaries
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
    }

    public void Reset()
    {
        isRunning = false;
    }
}

record class CameraState(GameStorage Storage) : IState
{
    static int FarLeftCursorBoundary = -Settings.TileWidth * Settings.TileSize;
    static int FarRightCursorBoundary = Settings.TileWidth * Settings.TileSize;
    static int FarTopCursorBoundary = -Settings.TileHeight * Settings.TileSize;
    static int FarBottomCursorBoundary = Settings.TileHeight * Settings.TileSize;

    Vector2 cursor = new(0, 0);

    public void HandleInput()
    {
        Camera2D cam = Storage.Camera;
        cam.rotation = 0f;
        if (Raylib.IsKeyPressed(KeyboardKey.KEY_MINUS))
        {
            cam.offset -= new Vector2(0.01f, 0.01f);
            cam.zoom -= 0.02f;
        }

        if (Raylib.IsKeyPressed(KeyboardKey.KEY_EQUAL))
        {
            cam.zoom += 0.01f;
        }

        if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT))
        {
            Vector2 delta = Raylib.GetMouseDelta();
            delta = delta * (-1.0f / cam.zoom);

            cursor += delta;
        }
        cursor.X = FMath.Bound(FarLeftCursorBoundary, FarRightCursorBoundary, cursor.X);
        cursor.Y = FMath.Bound(FarTopCursorBoundary, FarBottomCursorBoundary, cursor.Y);

        float wheel = Raylib.GetMouseWheelMove();
        if (wheel != 0)
        {
            Vector2 mouseWorldPos = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), cam);

            cam.offset = Raylib.GetMousePosition();

            cursor = mouseWorldPos;

            cam.zoom += wheel * 0.125f;
        }

        cam.zoom = FMath.Bound(0.375f, 3, cam.zoom);

        cam.target = cursor;
        Storage.Camera = cam;
    }

    public void Reset() { }
}

record class InDungeonState(GameStorage Storage) : IState
{
    MovementState Movement = new(Storage);
    CameraState Camera = new(Storage);

    public void HandleInput()
    {
        Movement.HandleInput();
        Camera.HandleInput();
    }

    public void Reset()
    {
        Movement.Reset();
        Camera.Reset();
    }

}

class GameState : IState
{
    public GameStorage GameStorage;
    InDungeonState InDungeon;

    private IState _active;
    IState Active { get => _active; set { Reset(); _active = value; } }

    public void Reset()
    {
        Active.Reset();
    }

    public void HandleInput()
    {
        Active.HandleInput();
    }

    public GameState(GameStorage storage)
    {
        GameStorage = storage;
        InDungeon = new(storage);
        _active = InDungeon;
    }
}


public sealed class Game
{
    GameState State;
    Settings Settings = Settings.Instance;
    GameStorage Storage = new();
    Artist Artist;

    public void Update()
    {
        State.HandleInput();

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

    public Game(Artist artist)
    {
        Resolution res = Settings.Instance.Resolution;
        res.InitWindow("RoguePound");
        State = new(Storage);
        Artist = artist;
    }
}
