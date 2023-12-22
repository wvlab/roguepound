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

record class IntraPersonalCommunicationState() : BattleState()
{
    private short Cooldown = 0;

    new public void HandleInput()
    {
        bool interacted = false;

        if (Cooldown == 0)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_PERIOD))
            {
                GameStorage.Player.Stats.Health = (int)Math.Min(
                    GameStorage.Player.Stats.Health + Math.Ceiling(GameStorage.Player.Stats.MaxHealth * 0.05),
                    GameStorage.Player.Stats.MaxHealth
                );

                interacted = true;
            }
        }


        if (Cooldown > 0) { Cooldown--; }

        if (interacted)
        {
            Cooldown = 60 * 5;
            return;
        }
        base.HandleInput();
    }
}

record class BattleState() : InteractState()
{
    private short Cooldown = 30;

    bool FindAndAttackMonster(int deltaX, int deltaY)
    {
        foreach (MonsterData mData in GameStorage.Monsters)
        {
            IMonster monster = mData.Monster;
            if (monster.Position.X == GameStorage.Player.Position.X + deltaX && monster.Position.Y == GameStorage.Player.Position.Y + deltaY)
            {
                ActorScene.Attack(GameStorage.Rand, GameStorage.Player, monster);
                return true;
            }
        }

        return false;
    }

    new public void HandleInput()
    {
        bool interacted = false;

        if (Cooldown == 0)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_K))
            {
                interacted = FindAndAttackMonster(0, -1);
            }
            else if (Raylib.IsKeyPressed(KeyboardKey.KEY_J))
            {
                interacted = FindAndAttackMonster(0, 1);
            }
            else if (Raylib.IsKeyPressed(KeyboardKey.KEY_H))
            {
                interacted = FindAndAttackMonster(-1, 0);
            }
            else if (Raylib.IsKeyPressed(KeyboardKey.KEY_L))
            {
                interacted = FindAndAttackMonster(1, 0);
            }
        }

        if (Cooldown > 0) { Cooldown--; }

        if (interacted)
        {
            Cooldown = 30;
            Enigmatologist.UpdateMonsters();
        }
        else base.HandleInput();
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
            GameStorage.Gold += (obj.Type as InteractiveObjectType.Coins)!.amount;
            return true;
        }

        return false;
    }
}

record class MovementState() : GenericDungeonInputState()
{
    new public void HandleInput()
    {
        Player Player = GameStorage.Player;
        bool isRunning = false;

        if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT))
        {
            isRunning = true;
        }

        int deltaX = 0;
        int deltaY = 0;

        if (Raylib.IsKeyPressed(KeyboardKey.KEY_K))
        {
            deltaY = -1;
        }
        else if (Raylib.IsKeyPressed(KeyboardKey.KEY_J))
        {
            deltaY = 1;
        }
        else if (Raylib.IsKeyPressed(KeyboardKey.KEY_H))
        {
            deltaX = -1;
        }
        else if (Raylib.IsKeyPressed(KeyboardKey.KEY_L))
        {
            deltaX = 1;
        }

        MovePlayer(deltaX, deltaY, isRunning, out bool moved);
        if (!moved)
        {
            base.HandleInput();
        }
    }

    void MovePlayer(int deltaX, int deltaY, bool isRunning, out bool moved)
    {
        moved = false;
        if (deltaX == 0 && deltaY == 0)
        {
            return;
        }
        int newY = 0;
        int newX = 0;
        int hp = GameStorage.Player.Stats.Health;
        do
        {
            (int x, int y) = GameStorage.Player.Position.ToTuple;
            newY = y + deltaY;
            newX = x + deltaX;

            if (newX == Settings.TileWidth || newX == -1)
            {
                return;
            }

            if (newY == Settings.TileHeight || newY == -1)
            {
                return;
            }

            if (!Tile.isTraversable(GameStorage.Tiles[newX, newY]))
            {
                return;
            }

            ActorScene.Move(GameStorage.Player, deltaX, deltaY);
            Enigmatologist.TakeTurn();
            moved = true;
        } while (
            isRunning
            && !GameStorage.Tiles[newX, newY].Type.IsDoor
            && hp == GameStorage.Player.Stats.Health
            && !GameStorage.InteractiveObjects.Any(obj => obj.Position.ToTuple == (newX, newY))
            && !GameStorage.Monsters.Any(md => md.Monster.Position.ToTuple == (newX + deltaX, newY + deltaY))
        );
    }
}

record class InDungeonState() : IState
{
    IntraPersonalCommunicationState Intra = new();

    public void HandleInput()
    {
        Intra.HandleInput();
        CameraHandler.UpdateCameraPos();
    }

    public void Reset()
    {
        Intra.Reset();
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

