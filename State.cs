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

    public static void HandleInput()
    {
        Camera2D cam = GameStorage.Camera;

        HandleZoom(ref cam);
        HandleCameraDrag(ref cam);

        if (Raylib.IsKeyPressed(KeyboardKey.KEY_F))
        {
            followPlayer = !followPlayer;
            GameStorage.CenterCamera();
        }

        GameStorage.Camera = cam;
    }

    public static void UpdateCameraPos()
    {
        if (followPlayer == true)
        {
            GameStorage.Camera.target = GameStorage.Player.Position.ToVector2 * Settings.TileSize;
        }
    }
}

record class GenericDungeonInputState : IState
{
    public void HandleInput()
    {
        CameraHandler.HandleInput();
    }

    public void Reset()
    {
    }
}

record class InteractState() : MovementState()
{
    new public void HandleInput()
    {
        bool interacted = false;

        foreach (var obj in GameStorage.InteractiveObjects.ToArray())
        {
            if (obj.Position == GameStorage.Player.Position)
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
                GameStorage.RegenerateDungeon();
                return true;
            }
        }

        return false;
    }

    private bool InteractWithCoins(InteractiveObject obj)
    {
        if (obj.Type.IsCoins)
        {
            GameStorage.InteractiveObjects.Remove(obj);
            GameStorage.Coins += (obj.Type as InteractiveObjectType.Coins)!.amount;
            return true;
        }

        return false;
    }
}

record class MovementState() : GenericDungeonInputState()
{
    short Cooldown = 0;
    Player Player = GameStorage.Player;
    new public void HandleInput()
    {
        if (Raylib.IsKeyPressed(KeyboardKey.KEY_K)
        && Cooldown == 0
        && Player.Position.Y != 0
        && Tile.isTraversable(GameStorage.Tiles[Player.Position.X, Player.Position.Y - 1])
        )
        {
            Player.Position.Y -= 1;
            Cooldown = 65;
        }

        else if (Raylib.IsKeyPressed(KeyboardKey.KEY_J)
        && Cooldown == 0
        && Player.Position.Y != Settings.TileHeight - 1
        && Tile.isTraversable(GameStorage.Tiles[Player.Position.X, Player.Position.Y + 1])
        )
        {
            Player.Position.Y += 1;
            Cooldown = 65;
        }

        else if (Raylib.IsKeyPressed(KeyboardKey.KEY_H)
        && Cooldown == 0
        && Player.Position.X != 0
        && Tile.isTraversable(GameStorage.Tiles[Player.Position.X - 1, Player.Position.Y])
        )
        {
            Player.Position.X -= 1;
            Cooldown = 65;
        }

        else if (Raylib.IsKeyPressed(KeyboardKey.KEY_L)
        && Cooldown == 0
        && Player.Position.X != Settings.TileWidth - 1
        && Tile.isTraversable(GameStorage.Tiles[Player.Position.X + 1, Player.Position.Y])
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
        Player = GameStorage.Player;
        base.Reset();
    }
}

record class InDungeonState() : IState
{
    InteractState Interact = new();

    public void HandleInput()
    {
        Interact.HandleInput();
        CameraHandler.UpdateCameraPos();
    }

    public void Reset()
    {
        Interact.Reset();
    }
}

class GameState : IState
{
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

    public GameState()
    {
        InDungeon = new();
        _active = InDungeon;
    }
}

