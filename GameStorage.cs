using System.Numerics;
using Raylib_CsLo;
using FunctionalRoguePound;

namespace RoguePound;

public interface IStorage
{
    void Reset();
}


public class GameStorage : IStorage
{
    public Tile[,] Tiles = new Tile[Settings.TileWidth, Settings.TileHeight];
    public Camera2D Camera = new Camera2D();
    public Player Player = new Player();
    public List<InteractiveObject> InteractiveObjects = new();
    public long Coins = 0;
    public Dungeon.Master Dungeon;

    public void CenterCamera()
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

    private void ResetTiles()
    {
        for (int x = 0; x < Settings.TileWidth; x += 1)
        {
            for (int y = 0; y < Settings.TileHeight; y += 1)
            {
                Tiles[x, y].Type = TileType.Void;
            }
        }
    }

    public void Reset()
    {
        Camera.zoom = 1.0f;
        Camera.rotation = 0.0f;
        InteractiveObjects.Clear();
        ResetTiles();
        Dungeon.Generate();
        CenterCamera();
    }

    public void RegenerateDungeon()
    {
        Reset();
    }

    public GameStorage()
    {
        Dungeon = new Dungeon.Master(ResetTiles, Tiles, InteractiveObjects, Player.Position);
        Reset();
    }
}

