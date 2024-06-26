using System.Numerics;
using Raylib_CsLo;
using FunctionalRoguePound;

namespace RoguePound;

public class MonsterData
{
    public IMonster Monster;
    public List<(int, int)> Path;
    public (int, int) LastPlayerPosition;

    public MonsterData(IMonster monster)
    {
        Monster = monster;
        Path = new();
        LastPlayerPosition = (-1, -1);
    }
}

public static class GameStorage
{
    static public Random Rand = new();
    static public Tile[,] Tiles = new Tile[Settings.TileWidth, Settings.TileHeight];
    static public Camera2D Camera = new();
    static public Player Player = new();
    static public List<InteractiveObject> InteractiveObjects = new();
    static public List<MonsterData> Monsters = new();
    static public List<Room> Rooms = new();
    static public long Gold = 0;
    static public int DungeonFloor = 0;
    static public Dungeon.Master Dungeon = new(ResetTiles);

    static public void CenterCamera()
    {
        Vector2 ScreenCenter = new Vector2(
            (float)Raylib.GetScreenWidth(),
            (float)Raylib.GetScreenHeight()
        ) / 2.0f;

        Vector2 MapCenter = new Vector2(
            (float)Settings.TileWidth,
            (float)Settings.TileHeight
        ) * Settings.TileSize / 2.0f;

        Camera.offset = ScreenCenter;
        Camera.target = Player.Position.ToVector2 * Settings.TileSize;
    }

    static private void ResetTiles()
    {
        for (int x = 0; x < Settings.TileWidth; x += 1)
        {
            for (int y = 0; y < Settings.TileHeight; y += 1)
            {
                Tiles[x, y].Type = TileType.Void;
                Tiles[x, y].IsOpen = false;
            }
        }
    }

    static public void Reset()
    {
        Player = new();
        Gold = 0;
        Camera.zoom = 1.0f;
        Camera.rotation = 0.0f;
        DungeonFloor = 0;
        RegenerateDungeon();
    }

    static public void RegenerateDungeon()
    {
        InteractiveObjects.Clear();
        Monsters.Clear();
        ResetTiles();
        Dungeon.Generate();
        DungeonFloor++;
        CenterCamera();
    }
}
