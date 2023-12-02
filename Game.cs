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

static class CameraHandler
{
    const float ZoomMultiplier = 0.125f;
    static int FarLeftCursorBoundary = -Settings.TileWidth * Settings.TileSize;
    static int FarRightCursorBoundary = Settings.TileWidth * Settings.TileSize;
    static int FarTopCursorBoundary = -Settings.TileHeight * Settings.TileSize;
    static int FarBottomCursorBoundary = Settings.TileHeight * Settings.TileSize;

    static Vector2 cursor = new(0, 0);
    static bool followPlayer = true;

    static void HandleZoomMouse(ref Camera2D cam)
    {
        float wheel = Raylib.GetMouseWheelMove();
        if (wheel != 0)
        {
            cam.target = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), cam);

            if (!followPlayer)
            {
                cam.offset = Raylib.GetMousePosition();
            }

            cam.zoom += wheel * ZoomMultiplier;
        }
    }

    static void HandleZoom(ref Camera2D cam)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.KEY_MINUS))
        {
            cam.zoom -= ZoomMultiplier;
        }

        if (Raylib.IsKeyPressed(KeyboardKey.KEY_EQUAL))
        {
            cam.zoom += ZoomMultiplier;
        }
        HandleZoomMouse(ref cam);

        cam.zoom = FMath.Bound(0.375f, 3, cam.zoom);
    }

    static void HandleCameraDrag(ref Camera2D cam)
    {
        if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT))
        {
            Vector2 delta = Raylib.GetMouseDelta();
            delta = delta * (-1.0f / cam.zoom);

            cam.target += delta;
        }

        cam.target.X = FMath.Bound(FarLeftCursorBoundary, FarRightCursorBoundary, cam.target.X);
        cam.target.Y = FMath.Bound(FarTopCursorBoundary, FarBottomCursorBoundary, cam.target.Y);
    }

    public static void HandleInput(GameStorage Storage)
    {
        Camera2D cam = Storage.Camera;

        HandleZoom(ref cam);
        HandleCameraDrag(ref cam);

        if (Raylib.IsKeyPressed(KeyboardKey.KEY_F))
        {
            followPlayer = !followPlayer;
            Storage.CenterCamera();
        }

        Storage.Camera = cam;
    }

    public static void UpdateCameraPos(GameStorage Storage)
    {
        if (followPlayer == true)
        {
            Storage.Camera.target = Storage.Player.Position.ToVector2 * Settings.TileSize;
        }
    }
}

record class GenericDungeonInputState : IState
{
    GameStorage Storage;

    public void HandleInput()
    {
        CameraHandler.HandleInput(Storage);
    }

    public void Reset()
    {
    }

    public GenericDungeonInputState(GameStorage storage)
    {
        Storage = storage;
    }
}

record class InteractState(GameStorage Storage) : MovementState(Storage)
{
    new public void HandleInput()
    {
        bool interacted = false;

        foreach (var obj in Storage.InteractiveObjects.ToArray())
        {
            if (obj.Position == Storage.Player.Position)
            {
                interacted = InteractWithCoins(obj)
                          || InteractWithStairs(obj);
            }
        }

        if (!interacted)
        {
            base.HandleInput();
        }
    }

    private bool InteractWithStairs(InteractiveObject obj)
    {
        if (obj.Type.IsStairs)
        {
            if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT) && Raylib.IsKeyPressed(KeyboardKey.KEY_PERIOD))
            {
                Storage.RegenerateDungeon();
                return true;
            }
        }

        return false;
    }

    private bool InteractWithCoins(InteractiveObject obj)
    {
        if (obj.Type.IsCoins)
        {
            Storage.InteractiveObjects.Remove(obj);
            Storage.Coins += (obj.Type as InteractiveObjectType.Coins)!.amount;
            return true;
        }

        return false;
    }
}

record class MovementState(GameStorage Storage) : GenericDungeonInputState(Storage)
{
    short Cooldown = 0;
    Player Player = Storage.Player;
    new public void HandleInput()
    {
        if (Raylib.IsKeyDown(KeyboardKey.KEY_K)
        && Cooldown == 0
        && Player.Position.Y != 0
        && Tile.isTraversable(Storage.Tiles[Player.Position.X, Player.Position.Y - 1])
        )
        {
            Player.Position.Y -= 1;
            Cooldown = 65;
        }

        else if (Raylib.IsKeyDown(KeyboardKey.KEY_J)
        && Cooldown == 0
        && Player.Position.Y != Settings.TileHeight - 1
        && Tile.isTraversable(Storage.Tiles[Player.Position.X, Player.Position.Y + 1])
        )
        {
            Player.Position.Y += 1;
            Cooldown = 65;
        }

        else if (Raylib.IsKeyDown(KeyboardKey.KEY_H)
        && Cooldown == 0
        && Player.Position.X != 0
        && Tile.isTraversable(Storage.Tiles[Player.Position.X - 1, Player.Position.Y])
        )
        {
            Player.Position.X -= 1;
            Cooldown = 65;
        }

        else if (Raylib.IsKeyDown(KeyboardKey.KEY_L)
        && Cooldown == 0
        && Player.Position.X != Settings.TileWidth - 1
        && Tile.isTraversable(Storage.Tiles[Player.Position.X + 1, Player.Position.Y])
        )
        {
            Player.Position.X += 1;
            Cooldown = 65;
        }
        else base.HandleInput();

        if (Cooldown > 0) Cooldown--;
    }

    new public void Reset()
    {
        Player = Storage.Player;
        base.Reset();
    }
}

record class InDungeonState(GameStorage Storage) : IState
{
    InteractState Interact = new(Storage);

    public void HandleInput()
    {
        Interact.HandleInput();
        CameraHandler.UpdateCameraPos(Storage);
    }

    public void Reset()
    {
        Interact.Reset();
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
    Artist Artist;
    GameStorage Storage;
    public void Update()
    {
        State.HandleInput();

        using (Artist.DrawingEnvironment())
        {
            Raylib.ClearBackground(Raylib.BLACK);

            using (Artist.World2DEnvironment(Storage.Camera))
            {
                Artist.DrawDungeon(Storage.Tiles);
                Artist.DrawInteractiveObjects(Storage.InteractiveObjects);
                Artist.DrawActor(Storage.Player);
            }
        }
    }

    public Game(Artist artist)
    {
        Resolution res = Settings.Instance.Resolution;
        res.InitWindow("RoguePound");
        Storage = new();
        State = new(Storage);
        Artist = artist;
    }
}
