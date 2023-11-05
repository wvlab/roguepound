using System.Numerics;
using System.Collections.Generic;
using Raylib_CsLo;
using FunctionalRoguePound;

namespace RoguePound;

public interface IArtist
{
    void DrawDungeon(in ITile[,] DungeonTiles);
}


public class Artist : IArtist
{
    public void DrawDungeon(in ITile[,] dungeonTiles)
    {
        for (int x = 0; x < Settings.TileWidth; x += 1)
        {
            for (int y = 0; y < Settings.TileHeight; y += 1)
            {
                Vector2 vec = new Vector2(
                    x * Settings.TileSize,
                    y * Settings.TileSize
                );
                dungeonTiles[x, y].Draw(vec);
            }
        }
    }

    public void DrawGrid()
    {
        for (int i = 0; i < Settings.TileWidth + 1; i++)
        {
            Raylib.DrawLineV(
                new Vector2(Settings.TileSize * i, 0),
                new Vector2(Settings.TileSize * i, Settings.TileHeight * Settings.TileSize),
                Raylib.LIGHTGRAY
            );
        }

        for (int i = 0; i < Settings.TileHeight + 1; i++)
        {
            Raylib.DrawLineV(
                new Vector2(0, Settings.TileSize * i),
                new Vector2(Settings.TileWidth * Settings.TileSize, Settings.TileSize * i),
                Raylib.LIGHTGRAY
            );
        }
    }

}


