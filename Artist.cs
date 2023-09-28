using System.Numerics;
using System.Collections.Generic;
using Raylib_CsLo;
using FunctionalRoguePound;

namespace RoguePound;

public interface IArtist
{
    void DrawTiles(in ITile[,] tiles);
}


public sealed class Artist : IArtist
{
    public void DrawTiles(in ITile[,] tiles)
    {
        for (int x = 0; x < Settings.TileWidth; x += 1)
        {
            for (int y = 0; y < Settings.TileHeight; y += 1)
            {
                Vector2 vec = new Vector2(
                    x * Settings.TileSize,
                    y * Settings.TileSize
                );
                tiles[x, y].Draw(vec);
            }
        }
    }
}


