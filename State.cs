using System.Numerics;
using Raylib_CsLo;

namespace RoguePound;

public interface IState
{
    void Reset();
}


public struct GameState : IState
{
    public ITile[,] Tiles = new ITile[Settings.TileWidth, Settings.TileHeight];
    public Camera2D Camera = new Camera2D();
    public Vector2 Cursor;
    Dungeon.Master Dungeon;

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
                Tiles[x, y] = new VoidTile();
            }
        }
    }

    public void Reset()
    {
        Cursor = new Vector2(0.0f, 0.0f);
        Camera.zoom = 1.0f;
        CenterCamera();
        ResetTiles();
        Dungeon.Generate();
    }

    public GameState()
    {
        Dungeon = new Dungeon.Master(ResetTiles, Tiles);
        Reset();
    }
}

