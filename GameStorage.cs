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
    Dungeon.Master Dungeon;
    long Coins = 0;

    private void CenterCamera()
    {
        Vector2 MapCenter = new Vector2(
            (float)(Settings.TileWidth * Settings.TileSize) / 2,
            (float)(Settings.TileHeight * Settings.TileSize) / 2
        );
        Vector2 ScreenCenter = new Vector2(
            (float)Raylib.GetScreenWidth() / 2,
            (float)Raylib.GetScreenHeight() / 2
        );
        Camera.offset = ScreenCenter - MapCenter;
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
        CenterCamera();
        ResetTiles();
        Dungeon.Generate();
        foreach (var obj in InteractiveObjects) Console.WriteLine(obj);
    }

    public GameStorage()
    {
        Dungeon = new Dungeon.Master(ResetTiles, Tiles, InteractiveObjects, Player.Position);
        Reset();
    }
}

